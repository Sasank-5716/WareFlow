using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wareflow.Application.Modules;

namespace Wareflow.Api.Authorization;

public sealed class ModuleRequirement(string moduleKey) : IAuthorizationRequirement
{
    public string ModuleKey { get; } = moduleKey;
}

public sealed class ModuleAuthorizationHandler(
    IModuleAccessChecker moduleAccessChecker)
    : AuthorizationHandler<ModuleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            await moduleAccessChecker.HasAccessAsync(
                requirement.ModuleKey,
                (context.Resource as HttpContext)?.RequestAborted ?? CancellationToken.None))
        {
            context.Succeed(requirement);
        }
    }
}

public static class ModuleEndpointExtensions
{
    public static RouteHandlerBuilder RequireModuleAccess(
        this RouteHandlerBuilder builder,
        string moduleKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleKey);
        builder.RequireAuthorization(policy =>
            policy.AddRequirements(new ModuleRequirement(moduleKey)));
        return builder;
    }
}
