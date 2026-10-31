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
    Member = 2
}

public enum TenantMembershipStatus
{
    Active = 1,
    Disabled = 2
}
