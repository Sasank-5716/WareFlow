namespace Wareflow.Application.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }

    Guid RequireTenantId();
}
