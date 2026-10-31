using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wareflow.Application.Persistence;
using Wareflow.Application.Tenancy;
using Wareflow.Infrastructure.Persistence;
using Wareflow.Infrastructure.Tenancy;

namespace Wareflow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.ConnectionString = config.GetConnectionString("WareflowDatabase");
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "ConnectionStrings:WareflowDatabase must be configured.")
            .ValidateOnStart();

        services.AddDbContext<WareflowDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

            options.UseNpgsql(databaseOptions.ConnectionString);
        });

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(serviceProvider =>
            serviceProvider.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantTransactionManager, TenantTransactionManager>();
        services.AddScoped<ITenantFoundationReader, TenantFoundationReader>();

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}

public sealed class DatabaseOptions
{
    public string? ConnectionString { get; set; }
}
