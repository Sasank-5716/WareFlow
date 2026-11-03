using Microsoft.EntityFrameworkCore;
using Wareflow.Application.Authorization;
using Wareflow.Application.Modules;
using Wareflow.Application.Tenancy;
using Wareflow.Domain.Modules;
using Wareflow.Infrastructure.Persistence;

namespace Wareflow.Infrastructure.Modules;

public sealed class TenantModuleAccessChecker(
    WareflowDbContext dbContext,
    ITenantContext tenantContext,
    IModuleCatalog catalog,
    IPermissionChecker permissionChecker,
    TimeProvider timeProvider) : IModuleAccessChecker
{
    public async Task<bool> HasAccessAsync(
        string moduleKey,
        CancellationToken cancellationToken)
    {
        var module = catalog.FindModule(moduleKey);
        var permissions = await permissionChecker.GetPermissionsAsync(cancellationToken);
        if (module is null ||
            module.Availability != ModuleAvailability.Available ||
            !module.RequiredPermissions.IsSubsetOf(permissions) ||
            !tenantContext.TenantId.HasValue)
        {
            return false;
        }

        var tenantId = tenantContext.TenantId.Value;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.tenant_id', {tenantId:N}, true)",
            cancellationToken);
        var settings = await dbContext.TenantModuleSettings
            .AsNoTracking()
            .ToDictionaryAsync(item => item.ModuleKey, StringComparer.Ordinal, cancellationToken);
        var entitlements = await dbContext.TenantSubscriptionEntitlements
            .AsNoTracking()
            .ToDictionaryAsync(item => item.ModuleKey, StringComparer.Ordinal, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var enabledModuleKeys = settings.Values
            .Where(setting => setting.Enabled)
            .Select(setting => setting.ModuleKey)
            .ToHashSet(StringComparer.Ordinal);
        var entitledModuleKeys = entitlements.Values
            .Where(entitlement => entitlement.IsValid(now))
            .Select(entitlement => entitlement.ModuleKey)
            .ToHashSet(StringComparer.Ordinal);
        return ModuleAccessEvaluator.HasAccess(
            catalog,
            module.Key,
            enabledModuleKeys,
            entitledModuleKeys,
            permissions);
    }
}
