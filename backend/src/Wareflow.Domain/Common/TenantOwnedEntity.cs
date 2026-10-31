namespace Wareflow.Domain.Common;

public abstract class TenantOwnedEntity : ITenantOwned, IHasUtcAuditMetadata
{
    protected TenantOwnedEntity(Guid tenantId, DateTime createdAtUtc)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        CreatedAtUtc = DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc);
        UpdatedAtUtc = CreatedAtUtc;
    }

    protected TenantOwnedEntity()
    {
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; protected set; }
}
