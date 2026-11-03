using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Wareflow.Application.Authentication;
using Wareflow.Application.Persistence;
using Wareflow.Application.Tenancy;
using Wareflow.Infrastructure.Persistence;
using Wareflow.Infrastructure.Tenancy;
using Wareflow.Infrastructure.Identity;
using Wareflow.Infrastructure.Authorization;
using Wareflow.Infrastructure.Auditing;
using Wareflow.Application.Authorization;
using Wareflow.Application.Auditing;

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

        services.AddOptions<IdentityEmailOptions>()
            .Bind(configuration.GetSection("Authentication:Email"))
            .Validate(options => Uri.TryCreate(
                    options.PublicOrigin,
                    UriKind.Absolute,
                    out _),
                "Authentication:Email:PublicOrigin must be an absolute URI.")
            .ValidateOnStart();
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection("Smtp"))
            .Validate(options => options.Port is > 0 and <= 65535,
                "Smtp:Port must be a valid TCP port.")
            .ValidateOnStart();

        services.AddIdentityCore<WareflowIdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<WareflowDbContext>()
            .AddSignInManager();
        services.AddScoped<IAuthenticationServiceProvider, IdentityAuthenticationService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(serviceProvider =>
            serviceProvider.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantTransactionManager, TenantTransactionManager>();
        services.AddScoped<ITenantFoundationReader, TenantFoundationReader>();
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantManagementService, TenantManagementService>();
        services.AddScoped<ITenantContextResolver, TenantManagementService>();
        services.AddSingleton<IAuthorizationCatalog, AuthorizationCatalog>();
        services.AddScoped<IPermissionChecker, TenantPermissionChecker>();
        services.AddScoped<IResourcePermissionChecker, ResourcePermissionChecker>();
        services.AddScoped<IWarehouseAssignmentChecker, DenyWarehouseAssignmentChecker>();
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();

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
