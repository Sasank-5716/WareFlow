namespace Wareflow.Application.Authorization;

public static class Permissions
{
    public const string ProductsView = "Products.View";
    public const string ProductsCreate = "Products.Create";
    public const string ProductsUpdate = "Products.Update";
    public const string ProductsArchive = "Products.Archive";
    public const string WarehousesView = "Warehouses.View";
    public const string WarehousesManage = "Warehouses.Manage";
    public const string InventoryView = "Inventory.View";
    public const string InventoryAdjust = "Inventory.Adjust";
    public const string InventoryTransfer = "Inventory.Transfer";
    public const string PurchasingView = "Purchasing.View";
    public const string PurchasingCreate = "Purchasing.Create";
    public const string PurchasingApprove = "Purchasing.Approve";
    public const string OrdersView = "Orders.View";
    public const string OrdersCreate = "Orders.Create";
    public const string OrdersApprove = "Orders.Approve";
    public const string ReportsView = "Reports.View";
    public const string ReportsExport = "Reports.Export";
    public const string UsersView = "Users.View";
    public const string UsersInvite = "Users.Invite";
    public const string UsersManage = "Users.Manage";
    public const string SettingsView = "Settings.View";
    public const string SettingsManage = "Settings.Manage";
    public const string ModulesView = "Modules.View";
    public const string ModulesManage = "Modules.Manage";
    public const string BillingView = "Billing.View";
    public const string BillingManage = "Billing.Manage";

    public static IReadOnlySet<string> All { get; } =
        new HashSet<string>(StringComparer.Ordinal)
        {
            ProductsView, ProductsCreate, ProductsUpdate, ProductsArchive,
            WarehousesView, WarehousesManage,
            InventoryView, InventoryAdjust, InventoryTransfer,
            PurchasingView, PurchasingCreate, PurchasingApprove,
            OrdersView, OrdersCreate, OrdersApprove,
            ReportsView, ReportsExport,
            UsersView, UsersInvite, UsersManage,
            SettingsView, SettingsManage,
            ModulesView, ModulesManage,
            BillingView, BillingManage
        };
}

public sealed record TenantRoleDefinition(
    string Name,
    IReadOnlySet<string> Permissions,
    bool CanAssignRoles);

public interface IAuthorizationCatalog
{
    IReadOnlyCollection<TenantRoleDefinition> Roles { get; }

    bool IsKnownPermission(string permission);

    bool IsKnownRole(string role);
}

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken);

    Task<IReadOnlySet<string>> GetPermissionsAsync(
        CancellationToken cancellationToken);

    Task<bool> CanAssignRoleAsync(
        string targetRole,
        CancellationToken cancellationToken);
}

public sealed record PermissionSnapshot(
    Guid TenantId,
    string Role,
    IReadOnlySet<string> Permissions);
