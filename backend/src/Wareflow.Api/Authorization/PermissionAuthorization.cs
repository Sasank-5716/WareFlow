using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wareflow.Application.Authorization;

namespace Wareflow.Api.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class PermissionAuthorizationHandler(
    IPermissionChecker permissionChecker)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            await permissionChecker.HasPermissionAsync(
                requirement.Permission,
                (context.Resource as HttpContext)?.RequestAborted ?? CancellationToken.None))
        {
            context.Succeed(requirement);
        }
    }
}

public static class PermissionEndpointExtensions
{
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        builder.RequireAuthorization(policy =>
            policy.AddRequirements(new PermissionRequirement(permission)));
        return builder;
    }
}
