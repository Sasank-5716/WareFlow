namespace Wareflow.Domain.Common;

public interface ITenantOwned
{
    Guid TenantId { get; }
}
