namespace Wareflow.Application.Authorization;

public sealed class AuthorizationCatalog : IAuthorizationCatalog
{
    private static readonly IReadOnlySet<string> ReadOnlyPermissions =
        Set(
            Permissions.ProductsView,
            Permissions.WarehousesView,
            Permissions.InventoryView,
            Permissions.PurchasingView,
            Permissions.OrdersView,
            Permissions.ReportsView,
            Permissions.UsersView,
            Permissions.SettingsView,
            Permissions.ModulesView,
            Permissions.BillingView);

    private static readonly IReadOnlySet<string> OperatorPermissions =
        Set(
            Permissions.ProductsView,
            Permissions.WarehousesView,
            Permissions.InventoryView,
            Permissions.InventoryAdjust,
            Permissions.InventoryTransfer);

    private static readonly IReadOnlySet<string> ManagerPermissions =
        Set(
            Permissions.ProductsView,
            Permissions.ProductsCreate,
            Permissions.ProductsUpdate,
            Permissions.ProductsArchive,
            Permissions.WarehousesView,
            Permissions.WarehousesManage,
            Permissions.InventoryView,
            Permissions.InventoryAdjust,
            Permissions.InventoryTransfer,
            Permissions.ReportsView,
            Permissions.UsersView,
            Permissions.SettingsView,
            Permissions.ModulesView);

    private static readonly IReadOnlySet<string> PurchasingPermissions =
        Set(
            Permissions.PurchasingView,
            Permissions.PurchasingCreate,
            Permissions.PurchasingApprove,
            Permissions.ReportsView);

    private static readonly IReadOnlySet<string> SalesPermissions =
        Set(
            Permissions.OrdersView,
            Permissions.OrdersCreate,
            Permissions.OrdersApprove,
            Permissions.ReportsView);

    private static readonly IReadOnlySet<string> AdministratorPermissions =
        Set(
            Permissions.ProductsView,
            Permissions.ProductsCreate,
            Permissions.ProductsUpdate,
            Permissions.ProductsArchive,
            Permissions.WarehousesView,
            Permissions.WarehousesManage,
            Permissions.InventoryView,
            Permissions.InventoryAdjust,
            Permissions.InventoryTransfer,
            Permissions.PurchasingView,
            Permissions.PurchasingCreate,
            Permissions.PurchasingApprove,
            Permissions.OrdersView,
            Permissions.OrdersCreate,
            Permissions.OrdersApprove,
            Permissions.ReportsView,
            Permissions.ReportsExport,
            Permissions.UsersView,
            Permissions.UsersInvite,
            Permissions.UsersManage,
            Permissions.SettingsView,
            Permissions.SettingsManage,
            Permissions.ModulesView,
            Permissions.ModulesManage);

    public IReadOnlyCollection<TenantRoleDefinition> Roles { get; } =
    [
        new("Owner", Permissions.All, true),
        new("CompanyAdministrator", AdministratorPermissions, true),
        new("WarehouseManager", ManagerPermissions, false),
        new("InventoryManager", ManagerPermissions, false),
        new("PurchasingManager", PurchasingPermissions, false),
        new("SalesManager", SalesPermissions, false),
        new("WarehouseOperator", OperatorPermissions, false),
        new("ReadOnlyUser", ReadOnlyPermissions, false),
        new("Member", ReadOnlyPermissions, false)
    ];

    public bool IsKnownPermission(string permission) =>
        Permissions.All.Contains(permission);

    public bool IsKnownRole(string role) =>
        Roles.Any(candidate => string.Equals(candidate.Name, role, StringComparison.Ordinal));

    private static IReadOnlySet<string> Set(params string[] permissions) =>
        new HashSet<string>(permissions, StringComparer.Ordinal);
}
