using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wareflow.Application;
using Wareflow.Application.Persistence;
using Wareflow.Application.Tenancy;
using Wareflow.Domain.Tenancy;
using Wareflow.Infrastructure;
using Wareflow.Infrastructure.Persistence;

namespace Wareflow.IntegrationTests;

public sealed class TenantIsolationTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [PostgresFact]
    public async Task Api_reads_only_the_current_tenant_settings()
    {
        var connectionString = GetConnectionStringOrUnavailable();
        Assert.NotNull(connectionString);
        await PrepareDatabaseAsync(connectionString);

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("ConnectionStrings:WareflowDatabase", connectionString);
            });

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Tenant-Id", TenantA.ToString());

        using var response = await client.GetAsync("/api/foundation/tenant-settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var settings = await response.Content.ReadFromJsonAsync<TenantSettingsResponse>();
        Assert.NotNull(settings);
        Assert.Equal(TenantA, settings.TenantId);
        Assert.NotEqual(TenantB, settings.TenantId);
    }

    [PostgresFact]
    public async Task Application_query_filter_cannot_read_another_tenant()
    {
        var connectionString = GetConnectionStringOrUnavailable();
        Assert.NotNull(connectionString);
        await PrepareDatabaseAsync(connectionString);

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:WareflowDatabase"] = connectionString
            })
            .Build());
        services.AddSingleton<ITenantContext>(new FixedTenantContext(TenantA));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var store = scope.ServiceProvider.GetRequiredService<ITenantFoundationStore>();
        var settings = await store.GetSettingsAsync();

        Assert.NotNull(settings);
        Assert.Equal(TenantA, settings.TenantId);
        Assert.NotEqual(TenantB, settings.TenantId);
    }

    [PostgresFact]
    public async Task Rls_rejects_a_write_for_a_different_tenant()
    {
        var connectionString = GetConnectionStringOrUnavailable();
        Assert.NotNull(connectionString);
        await PrepareDatabaseAsync(connectionString);

        var services = new ServiceCollection();
        services.AddInfrastructure(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:WareflowDatabase"] = connectionString
            })
            .Build());
        services.AddSingleton<ITenantContext>(new FixedTenantContext(TenantA));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<WareflowDbContext>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITenantTransactionManager>();

        await using var transaction = await transactionManager.BeginAsync();
        db.TenantSettings.Add(new TenantSettings(
            TenantB,
            "UTC",
            "en-US",
            "USD",
            DateTime.UtcNow));

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static string? GetConnectionStringOrUnavailable()
    {
        return Environment.GetEnvironmentVariable("WAREFLOW_TEST_CONNECTION_STRING");
    }

    private static async Task PrepareDatabaseAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<WareflowDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var db = new WareflowDbContext(options, new FixedTenantContext(null));
        await db.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await DeleteTenantDataAsync(connection, TenantA);
        await DeleteTenantDataAsync(connection, TenantB);
        await ExecuteAsync(connection, $"""
            INSERT INTO tenants ("Id", "Name", "Slug", "CreatedAtUtc", "UpdatedAtUtc")
            VALUES
                ('{TenantA}', 'Tenant A', 'tenant-a', NOW(), NOW()),
                ('{TenantB}', 'Tenant B', 'tenant-b', NOW(), NOW());
            """);

        await InsertSettingsAsync(connection, TenantA);
        await InsertSettingsAsync(connection, TenantB);
    }

    private static async Task DeleteTenantDataAsync(
        NpgsqlConnection connection,
        Guid tenantId)
    {
        await using var transaction = await connection.BeginTransactionAsync();
        await using (var setTenant = new NpgsqlCommand(
            "SELECT set_config('app.tenant_id', @tenantId, true)",
            connection,
            transaction))
        {
            setTenant.Parameters.AddWithValue("tenantId", tenantId.ToString());
            await setTenant.ExecuteNonQueryAsync();
        }

        await using (var delete = new NpgsqlCommand("""
            DELETE FROM tenant_settings WHERE "TenantId" = @tenantId;
            DELETE FROM tenant_memberships WHERE "TenantId" = @tenantId;
            DELETE FROM audit_logs WHERE "TenantId" = @tenantId;
            """, connection, transaction))
        {
            delete.Parameters.AddWithValue("tenantId", tenantId);
            await delete.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();

        await using var deleteTenant = new NpgsqlCommand(
            "DELETE FROM tenants WHERE \"Id\" = @tenantId",
            connection);
        deleteTenant.Parameters.AddWithValue("tenantId", tenantId);
        await deleteTenant.ExecuteNonQueryAsync();
    }

    private static async Task InsertSettingsAsync(NpgsqlConnection connection, Guid tenantId)
    {
        await using var transaction = await connection.BeginTransactionAsync();
        await using (var setTenant = new NpgsqlCommand(
            "SELECT set_config('app.tenant_id', @tenantId, true)",
            connection,
            transaction))
        {
            setTenant.Parameters.AddWithValue("tenantId", tenantId.ToString());
            await setTenant.ExecuteNonQueryAsync();
        }

        await using var insert = new NpgsqlCommand("""
            INSERT INTO tenant_settings
                ("Id", "TimeZoneId", "Locale", "DefaultCurrencyCode", "TenantId", "CreatedAtUtc", "UpdatedAtUtc")
            VALUES (@id, 'UTC', 'en-US', 'USD', @tenantId, NOW(), NOW())
            ON CONFLICT ("TenantId") DO NOTHING
            """, connection, transaction);
        insert.Parameters.AddWithValue("id", Guid.NewGuid());
        insert.Parameters.AddWithValue("tenantId", tenantId);
        await insert.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class FixedTenantContext(Guid? tenantId) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;

        public Guid RequireTenantId() =>
            TenantId ?? throw new InvalidOperationException("Tenant context is unavailable.");
    }

    private sealed record TenantSettingsResponse(
        Guid TenantId,
        string TimeZoneId,
        string Locale,
        string DefaultCurrencyCode);

    private sealed class PostgresFactAttribute : FactAttribute
    {
        public PostgresFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(
                    Environment.GetEnvironmentVariable("WAREFLOW_TEST_CONNECTION_STRING")))
            {
                Skip = "WAREFLOW_TEST_CONNECTION_STRING is not configured.";
            }
        }
    }
}
