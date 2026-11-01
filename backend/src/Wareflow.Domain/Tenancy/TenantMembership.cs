using Wareflow.Domain.Common;

namespace Wareflow.Domain.Tenancy;

public sealed class TenantMembership : TenantOwnedEntity
{
    private TenantMembership()
    {
    }

    public TenantMembership(
        Guid tenantId,
        string identityUserId,
        TenantMembershipRole role,
        DateTime createdAtUtc)
        : base(tenantId, createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            throw new ArgumentException("Identity user ID is required.", nameof(identityUserId));
        }

        IdentityUserId = identityUserId.Trim();
        Role = role;
        Status = TenantMembershipStatus.Active;
    }

    public string IdentityUserId { get; private set; } = null!;

    public TenantMembershipRole Role { get; private set; }

    public TenantMembershipStatus Status { get; private set; }

    public void Disable() => Status = TenantMembershipStatus.Disabled;

    public void Enable() => Status = TenantMembershipStatus.Active;

    public void ChangeRole(TenantMembershipRole role) => Role = role;
}

public enum TenantMembershipRole
{
    Owner = 1,
    CompanyAdministrator = 2,
    WarehouseManager = 3,
    InventoryManager = 4,
    PurchasingManager = 5,
    SalesManager = 6,
    WarehouseOperator = 7,
    ReadOnlyUser = 8,
    [Obsolete("Use ReadOnlyUser for memberships created by older migrations.")]
    Member = 9
}

public enum TenantMembershipStatus
{
    Active = 1,
    Disabled = 2
}
