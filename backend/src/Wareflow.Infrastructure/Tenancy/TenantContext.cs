using Wareflow.Application.Tenancy;

namespace Wareflow.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public Guid RequireTenantId() =>
        TenantId ?? throw new InvalidOperationException("A tenant context is required.");

    internal void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        if (TenantId.HasValue && TenantId.Value != tenantId)
        {
            throw new InvalidOperationException("Tenant context cannot be changed within a scope.");
        }

        TenantId = tenantId;
    }
}
