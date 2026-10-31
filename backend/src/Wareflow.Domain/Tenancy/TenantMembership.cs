using Wareflow.Domain.Common;

namespace Wareflow.Domain.Tenancy;

public sealed class TenantMembership : TenantOwnedEntity
{
    private TenantMembership()
    {
    }

    public TenantMembership(Guid tenantId, string identityUserId, DateTime createdAtUtc)
        : base(tenantId, createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            throw new ArgumentException("Identity user ID is required.", nameof(identityUserId));
        }

        IdentityUserId = identityUserId.Trim();
    }

    public string IdentityUserId { get; private set; } = null!;
}
