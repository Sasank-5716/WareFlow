namespace Wareflow.Application.Authorization;

public interface ITenantResource
{
    Guid TenantId { get; }

    string? OwnerIdentityUserId { get; }

    Guid? WarehouseId { get; }
}

public interface IWarehouseAssignmentChecker
{
    Task<bool> IsAssignedAsync(
        Guid tenantId,
        string identityUserId,
        Guid warehouseId,
        CancellationToken cancellationToken);
}

public interface IResourcePermissionChecker
{
    Task<bool> CanAccessAsync<TResource>(
        TResource resource,
        string permission,
        CancellationToken cancellationToken)
        where TResource : ITenantResource;
}
