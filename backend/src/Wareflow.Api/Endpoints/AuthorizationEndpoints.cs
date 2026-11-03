using Wareflow.Application.Authorization;

namespace Wareflow.Api.Endpoints;

public static class AuthorizationEndpoints
{
    public static IEndpointRouteBuilder MapAuthorizationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/authorization/catalog", (
            IAuthorizationCatalog catalog) =>
            Results.Ok(catalog.Roles))
            .RequireAuthorization()
            .WithTags("Authorization")
            .WithSummary("Get the tenant role and permission catalog")
            .WithDescription("Returns platform-defined tenant roles and permissions. It never grants access.")
            .Produces<IReadOnlyCollection<TenantRoleDefinition>>();

        endpoints.MapGet("/api/tenants/active/permissions", async (
            IPermissionChecker checker,
            Wareflow.Application.Tenancy.ITenantContext context,
            CancellationToken cancellationToken) =>
        {
            if (!context.TenantId.HasValue)
            {
                return Results.NotFound();
            }

            var role = await checker.GetCurrentRoleAsync(cancellationToken);
            if (role is null)
            {
                return Results.NotFound();
            }

            var permissions = await checker.GetPermissionsAsync(cancellationToken);
            return Results.Ok(new PermissionSnapshot(
                context.TenantId.Value,
                role,
                permissions));
        })
        .RequireAuthorization()
        .WithTags("Authorization")
        .WithSummary("Get permissions in the active tenant")
        .Produces<PermissionSnapshot>()
        .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }
}
