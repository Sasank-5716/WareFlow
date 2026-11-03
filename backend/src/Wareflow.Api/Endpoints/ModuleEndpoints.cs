using Wareflow.Api.Authorization;
using Wareflow.Application.Authorization;
using Wareflow.Application.Modules;

namespace Wareflow.Api.Endpoints;

public static class ModuleEndpoints
{
    public static IEndpointRouteBuilder MapModuleEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/modules/catalog", (IModuleCatalog catalog) =>
            Results.Ok(new ModuleCatalogSnapshot(catalog.Modules, catalog.Plans)))
            .RequireAuthorization()
            .WithTags("Modules")
            .WithSummary("List the platform module catalog")
            .WithDescription("Returns stable module definitions and plan eligibility. It does not grant tenant access.")
            .Produces<ModuleCatalogSnapshot>();

        var tenantGroup = endpoints.MapGroup("/api/tenants/active/modules")
            .RequireAuthorization()
            .WithTags("Modules");

        tenantGroup.MapGet("/", async (
            IModuleService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetEffectiveModulesAsync(cancellationToken)))
            .RequirePermission(Permissions.ModulesView)
            .WithSummary("List effective modules for the active tenant")
            .Produces<IReadOnlyCollection<TenantModuleSnapshot>>();

        tenantGroup.MapPut("/{moduleKey}", async (
            string moduleKey,
            UpdateTenantModuleRequest request,
            IModuleService service,
            CancellationToken cancellationToken) =>
            await service.SetModuleEnabledAsync(moduleKey, request.Enabled, cancellationToken)
                ? Results.NoContent()
                : Results.BadRequest(new { error = "invalid_module_configuration" }))
            .RequirePermission(Permissions.ModulesManage)
            .WithSummary("Enable or disable a tenant module")
            .WithDescription("Disabling a module only changes access; historical business data is preserved.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        endpoints.MapGet("/api/tenants/active/subscription", async (
            IModuleService service,
            CancellationToken cancellationToken) =>
        {
            var subscription = await service.GetSubscriptionAsync(cancellationToken);
            return subscription is null ? Results.NotFound() : Results.Ok(subscription);
        })
        .RequireAuthorization()
        .RequirePermission(Permissions.BillingView)
        .WithTags("Modules")
        .WithSummary("Get the active tenant subscription entitlement")
        .Produces<TenantSubscriptionSnapshot>()
        .Produces(StatusCodes.Status404NotFound);

        endpoints.MapPut("/api/tenants/active/subscription/plan", async (
            AssignSubscriptionPlanRequest request,
            IModuleService service,
            CancellationToken cancellationToken) =>
        {
            var subscription = await service.AssignPlanAsync(request, cancellationToken);
            return subscription is null
                ? Results.BadRequest(new { error = "invalid_plan_or_permission" })
                : Results.Ok(subscription);
        })
        .RequireAuthorization()
        .RequirePermission(Permissions.BillingManage)
        .WithTags("Modules")
        .WithSummary("Assign a platform subscription plan")
        .WithDescription("Development subscription assignment only; payment-provider integration is not performed.")
        .Produces<TenantSubscriptionSnapshot>()
        .Produces(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}
