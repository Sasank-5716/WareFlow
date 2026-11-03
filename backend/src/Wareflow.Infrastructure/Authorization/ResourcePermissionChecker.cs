using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wareflow.Application.Authorization;
using Wareflow.Application.Tenancy;

namespace Wareflow.Infrastructure.Authorization;

public sealed class ResourcePermissionChecker(
    IPermissionChecker permissionChecker,
    IWarehouseAssignmentChecker warehouseAssignmentChecker,
    ITenantContext tenantContext,
    IHttpContextAccessor httpContextAccessor) : IResourcePermissionChecker
{
    public async Task<bool> CanAccessAsync<TResource>(
        TResource resource,
        string permission,
        CancellationToken cancellationToken)
        where TResource : ITenantResource
    {
        var tenantId = tenantContext.TenantId;
        var identityUserId = httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);
        if (!tenantId.HasValue ||
            tenantId.Value != resource.TenantId ||
            string.IsNullOrWhiteSpace(identityUserId) ||
            !await permissionChecker.HasPermissionAsync(permission, cancellationToken))
        {
            return false;
        }

        if (string.Equals(
                resource.OwnerIdentityUserId,
                identityUserId,
                StringComparison.Ordinal))
        {
            return true;
        }

        return resource.WarehouseId.HasValue &&
            await warehouseAssignmentChecker.IsAssignedAsync(
                resource.TenantId,
                identityUserId,
                resource.WarehouseId.Value,
                cancellationToken);
    }
}

public sealed class DenyWarehouseAssignmentChecker : IWarehouseAssignmentChecker
{
    public Task<bool> IsAssignedAsync(
        Guid tenantId,
        string identityUserId,
        Guid warehouseId,
        CancellationToken cancellationToken) =>
        Task.FromResult(false);
}
