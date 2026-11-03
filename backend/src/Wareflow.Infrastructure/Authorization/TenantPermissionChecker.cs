using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wareflow.Application.Authorization;
using Wareflow.Application.Tenancy;
using Wareflow.Domain.Tenancy;
using Wareflow.Infrastructure.Persistence;

namespace Wareflow.Infrastructure.Authorization;

public sealed class TenantPermissionChecker(
    WareflowDbContext dbContext,
    ITenantContext tenantContext,
    IHttpContextAccessor httpContextAccessor,
    IAuthorizationCatalog catalog) : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken)
    {
        if (!catalog.IsKnownPermission(permission) ||
            !tenantContext.TenantId.HasValue)
        {
            return false;
        }

        var role = await GetActiveRoleAsync(cancellationToken);
        return role is not null &&
               catalog.Roles
                   .FirstOrDefault(candidate => candidate.Name == role)
                   ?.Permissions.Contains(permission) == true;
    }

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(
        CancellationToken cancellationToken)
    {
        var role = await GetActiveRoleAsync(cancellationToken);
        return role is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : catalog.Roles
                .FirstOrDefault(candidate => candidate.Name == role)
                ?.Permissions
                ?? new HashSet<string>(StringComparer.Ordinal);
    }

    public async Task<string?> GetCurrentRoleAsync(
        CancellationToken cancellationToken) =>
        await GetActiveRoleAsync(cancellationToken);

    public async Task<bool> CanAssignRoleAsync(
        string targetRole,
        CancellationToken cancellationToken)
    {
        var targetDefinition = catalog.Roles
            .FirstOrDefault(candidate => candidate.Name == targetRole);
        if (targetDefinition is null)
        {
            return false;
        }

        var currentRole = await GetActiveRoleAsync(cancellationToken);
        if (targetRole == nameof(TenantMembershipRole.Owner))
        {
            return currentRole == nameof(TenantMembershipRole.Owner);
        }

        return await CanDelegatePermissionsAsync(
            targetDefinition.Permissions,
            cancellationToken);
    }

    public async Task<bool> CanDelegatePermissionsAsync(
        IEnumerable<string> permissions,
        CancellationToken cancellationToken)
    {
        var requestedPermissions = permissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .ToHashSet(StringComparer.Ordinal);
        if (requestedPermissions.Count == 0 ||
            requestedPermissions.Any(permission => !catalog.IsKnownPermission(permission)))
        {
            return false;
        }

        var currentRole = await GetActiveRoleAsync(cancellationToken);
        var definition = currentRole is null
            ? null
            : catalog.Roles.FirstOrDefault(candidate => candidate.Name == currentRole);

        return definition is not null &&
               definition.CanAssignRoles &&
               requestedPermissions.IsSubsetOf(definition.Permissions);
    }

    private async Task<string?> GetActiveRoleAsync(
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = tenantContext.TenantId;
        if (string.IsNullOrWhiteSpace(userId) || !tenantId.HasValue)
        {
            return null;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.tenant_id', {tenantId.Value:N}, true)",
            cancellationToken);
        var role = await dbContext.TenantMemberships
            .IgnoreQueryFilters()
            .Where(membership =>
                membership.TenantId == tenantId.Value &&
                membership.IdentityUserId == userId &&
                membership.Status == TenantMembershipStatus.Active)
            .Select(membership => membership.Role.ToString())
            .SingleOrDefaultAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return role;
    }
}
