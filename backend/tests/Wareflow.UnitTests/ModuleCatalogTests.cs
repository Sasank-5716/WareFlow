using Wareflow.Application.Modules;
using Wareflow.Application.Authorization;
using Wareflow.Domain.Modules;

namespace Wareflow.UnitTests;

public sealed class ModuleCatalogTests
{
    [Fact]
    public void Initial_catalog_contains_stable_keys_and_valid_dependencies()
    {
        var catalog = new ModuleCatalog();

        Assert.Equal(21, catalog.Modules.Count);
        Assert.All(catalog.Modules, module =>
        {
            Assert.DoesNotContain(' ', module.Key);
            Assert.NotEmpty(module.DisplayName);
            Assert.NotEmpty(module.RequiredPermissions);
        });
        catalog.Validate();
    }

    [Fact]
    public void Plans_define_module_eligibility_without_customer_accounts()
    {
        var catalog = new ModuleCatalog();

        Assert.Contains(catalog.Plans, plan => plan.Key == "starter");
        Assert.DoesNotContain(
            catalog.Plans.Single(plan => plan.Key == "starter").IncludedModuleKeys,
            key => key == "advanced_analytics");
        Assert.Contains(
            catalog.Plans.Single(plan => plan.Key == "enterprise").IncludedModuleKeys,
            key => key == "api_integrations");
    }

    [Fact]
    public void Effective_access_inputs_are_separate()
    {
        var catalog = new ModuleCatalog();
        var advancedAnalytics = catalog.FindModule("advanced_analytics");
        var setting = new TenantModuleSetting(
            Guid.NewGuid(),
            "advanced_analytics",
            enabled: true,
            DateTime.UtcNow);
        var entitlement = new TenantSubscriptionEntitlement(
            setting.TenantId,
            "enterprise",
            "advanced_analytics",
            included: true,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow);

        Assert.NotNull(advancedAnalytics);
        Assert.Contains(Permissions.ReportsView, advancedAnalytics.RequiredPermissions);
        Assert.Contains(Permissions.ReportsExport, advancedAnalytics.RequiredPermissions);
        Assert.True(setting.Enabled);
        Assert.True(entitlement.IsValid(DateTime.UtcNow));
        Assert.False(entitlement.IsValid(DateTime.UtcNow.AddDays(2)));
    }

    [Fact]
    public void Effective_access_denies_disabled_unentitled_dependent_and_unauthorized_modules()
    {
        var catalog = new ModuleCatalog();
        var enabled = new HashSet<string>(["products", "warehouses", "inventory"]);
        var entitled = new HashSet<string>(["products", "warehouses", "inventory"]);
        var inventoryPermissions = new HashSet<string>([Permissions.InventoryView]);

        Assert.True(ModuleAccessEvaluator.HasAccess(
            catalog,
            "inventory",
            enabled,
            entitled,
            inventoryPermissions));

        enabled.Remove("warehouses");
        Assert.False(ModuleAccessEvaluator.HasAccess(
            catalog,
            "inventory",
            enabled,
            entitled,
            inventoryPermissions));

        enabled.Add("warehouses");
        entitled.Remove("inventory");
        Assert.False(ModuleAccessEvaluator.HasAccess(
            catalog,
            "inventory",
            enabled,
            entitled,
            inventoryPermissions));

        entitled.Add("inventory");
        Assert.False(ModuleAccessEvaluator.HasAccess(
            catalog,
            "inventory",
            enabled,
            entitled,
            new HashSet<string>()));
    }
}
