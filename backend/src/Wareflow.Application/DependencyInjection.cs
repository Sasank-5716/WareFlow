using Microsoft.Extensions.DependencyInjection;
using Wareflow.Application.Authentication;
using Wareflow.Application.Foundation;
using Wareflow.Application.Tenancy;

namespace Wareflow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IFoundationProbeValidator, FoundationProbeValidator>();
        services.AddScoped<ITenantFoundationStore, TenantFoundationStore>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
