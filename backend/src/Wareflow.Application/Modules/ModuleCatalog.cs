using Wareflow.Application.Authorization;

namespace Wareflow.Application.Modules;

public enum ModuleAvailability
{
    Available = 1,
    ComingSoon = 2,
    Retired = 3
}

public sealed record ModuleDefinition(
    string Key,
    string DisplayName,
    string Description,
    string Category,
    string IconKey,
    IReadOnlySet<string> Dependencies,
    IReadOnlySet<string> RequiredPermissions,
    ModuleAvailability Availability,
    IReadOnlySet<string> EligiblePlanKeys);

public sealed record SubscriptionPlanDefinition(
    string Key,
    string DisplayName,
    IReadOnlySet<string> IncludedModuleKeys);

public interface IModuleCatalog
{
    IReadOnlyCollection<ModuleDefinition> Modules { get; }

    IReadOnlyCollection<SubscriptionPlanDefinition> Plans { get; }

    ModuleDefinition? FindModule(string key);

    SubscriptionPlanDefinition? FindPlan(string key);

    void Validate();
}

public sealed class ModuleCatalog : IModuleCatalog
{
    public ModuleCatalog()
    {
        Validate();
    }

    private static readonly IReadOnlyCollection<ModuleDefinition> InitialModules =
    [
        Module("products", "Products", "Product catalog and lifecycle management.", "Catalog", "products", [], [Permissions.ProductsView], ["starter", "professional", "enterprise"]),
        Module("inventory", "Inventory", "Stock balances and inventory movements.", "Inventory", "inventory", ["products", "warehouses"], [Permissions.InventoryView], ["starter", "professional", "enterprise"]),
        Module("warehouses", "Warehouses", "Warehouse and location management.", "Operations", "warehouses", [], [Permissions.WarehousesView], ["starter", "professional", "enterprise"]),
        Module("multiple_warehouses", "Multiple Warehouses", "Operate across multiple warehouses.", "Operations", "multiple-warehouses", ["warehouses"], [Permissions.WarehousesManage], ["professional", "enterprise"]),
        Module("suppliers", "Suppliers", "Supplier records and relationships.", "Purchasing", "suppliers", [], [Permissions.PurchasingView], ["starter", "professional", "enterprise"]),
        Module("customers", "Customers", "Customer records and relationships.", "Sales", "customers", [], [Permissions.OrdersView], ["starter", "professional", "enterprise"]),
        Module("purchasing", "Purchasing", "Purchase orders and approvals.", "Purchasing", "purchasing", ["suppliers"], [Permissions.PurchasingView], ["professional", "enterprise"]),
        Module("receiving", "Receiving", "Inbound receiving workflows.", "Inventory", "receiving", ["purchasing", "inventory"], [Permissions.InventoryView, Permissions.PurchasingView], ["professional", "enterprise"]),
        Module("stock_transfers", "Stock Transfers", "Transfer stock between warehouses.", "Inventory", "stock-transfers", ["inventory", "multiple_warehouses"], [Permissions.InventoryTransfer], ["professional", "enterprise"]),
        Module("orders", "Orders", "Sales order management.", "Sales", "orders", ["customers"], [Permissions.OrdersView], ["starter", "professional", "enterprise"]),
        Module("picking_packing", "Picking & Packing", "Pick and pack customer orders.", "Operations", "picking-packing", ["orders", "inventory"], [Permissions.OrdersView, Permissions.InventoryView], ["professional", "enterprise"]),
        Module("shipping", "Shipping", "Shipment preparation and dispatch.", "Operations", "shipping", ["orders"], [Permissions.OrdersView], ["professional", "enterprise"]),
        Module("returns", "Returns", "Customer return workflows.", "Sales", "returns", ["orders"], [Permissions.OrdersView, Permissions.InventoryView], ["professional", "enterprise"]),
        Module("stock_counts", "Stock Counts", "Cycle counts and stock reconciliation.", "Inventory", "stock-counts", ["inventory"], [Permissions.InventoryView, Permissions.InventoryAdjust], ["professional", "enterprise"]),
        Module("barcode_qr", "Barcode & QR", "Barcode and QR workflows.", "Inventory", "barcode-qr", ["products"], [Permissions.ProductsView], ["professional", "enterprise"]),
        Module("batch_tracking", "Batch Tracking", "Track inventory by batch.", "Inventory", "batch-tracking", ["products", "inventory"], [Permissions.InventoryView], ["professional", "enterprise"]),
        Module("serial_tracking", "Serial Tracking", "Track inventory by serial number.", "Inventory", "serial-tracking", ["products", "inventory"], [Permissions.InventoryView], ["professional", "enterprise"]),
        Module("reports_analytics", "Reports & Analytics", "Operational reports and dashboards.", "Analytics", "reports", [], [Permissions.ReportsView], ["starter", "professional", "enterprise"]),
        Module("advanced_analytics", "Advanced Analytics", "Advanced analytics and insights.", "Analytics", "advanced-analytics", ["reports_analytics"], [Permissions.ReportsView, Permissions.ReportsExport], ["enterprise"]),
        Module("warehouse_3d", "3D Warehouse View", "Three-dimensional warehouse visualization.", "Operations", "warehouse-3d", ["warehouses"], [Permissions.WarehousesView], ["enterprise"]),
        Module("api_integrations", "API Integrations", "External API and integration access.", "Integrations", "api-integrations", [], [Permissions.ModulesManage], ["enterprise"])
    ];

    public IReadOnlyCollection<ModuleDefinition> Modules => InitialModules;

    public IReadOnlyCollection<SubscriptionPlanDefinition> Plans { get; } =
    [
        new("starter", "Starter", InitialModules.Where(module => module.EligiblePlanKeys.Contains("starter")).Select(module => module.Key).ToHashSet(StringComparer.Ordinal)),
        new("professional", "Professional", InitialModules.Where(module => module.EligiblePlanKeys.Contains("professional")).Select(module => module.Key).ToHashSet(StringComparer.Ordinal)),
        new("enterprise", "Enterprise", InitialModules.Where(module => module.EligiblePlanKeys.Contains("enterprise")).Select(module => module.Key).ToHashSet(StringComparer.Ordinal))
    ];

    public ModuleDefinition? FindModule(string key) =>
        Modules.FirstOrDefault(module => string.Equals(module.Key, key, StringComparison.Ordinal));

    public SubscriptionPlanDefinition? FindPlan(string key) =>
        Plans.FirstOrDefault(plan => string.Equals(plan.Key, key, StringComparison.Ordinal));

    public void Validate()
    {
        var moduleKeys = Modules.Select(module => module.Key).ToHashSet(StringComparer.Ordinal);
        var planKeys = Plans.Select(plan => plan.Key).ToHashSet(StringComparer.Ordinal);
        if (moduleKeys.Count != Modules.Count ||
            planKeys.Count != Plans.Count ||
            Modules.Any(module =>
                module.Dependencies.Any(dependency => !moduleKeys.Contains(dependency)) ||
                module.RequiredPermissions.Any(permission => !Permissions.All.Contains(permission)) ||
                module.EligiblePlanKeys.Any(plan => !planKeys.Contains(plan))))
        {
            throw new InvalidOperationException("The module catalog contains an invalid reference.");
        }

        foreach (var module in Modules)
        {
            Visit(module.Key, []);
        }

        void Visit(string key, HashSet<string> path)
        {
            if (!path.Add(key))
            {
                throw new InvalidOperationException("The module catalog contains a dependency cycle.");
            }

            var module = FindModule(key)
                ?? throw new InvalidOperationException("The module catalog contains an invalid dependency.");
            foreach (var dependency in module.Dependencies)
            {
                Visit(dependency, path);
            }

            path.Remove(key);
        }
    }

    private static ModuleDefinition Module(
        string key,
        string displayName,
        string description,
        string category,
        string iconKey,
        IEnumerable<string> dependencies,
        IEnumerable<string> permissions,
        IEnumerable<string> plans) =>
        new(
            key,
            displayName,
            description,
            category,
            iconKey,
            dependencies.ToHashSet(StringComparer.Ordinal),
            permissions.ToHashSet(StringComparer.Ordinal),
            ModuleAvailability.Available,
            plans.ToHashSet(StringComparer.Ordinal));
}
