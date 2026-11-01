using Microsoft.AspNetCore.Authorization;
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
        if (await permissionChecker.HasPermissionAsync(
                requirement.Permission,
                CancellationToken.None))
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
        builder.RequireAuthorization(policy =>
            policy.AddRequirements(new PermissionRequirement(permission)));
        return builder;
    }
}
