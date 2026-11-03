using Microsoft.EntityFrameworkCore;
using Wareflow.Application.Auditing;
using Wareflow.Application.Authorization;
using Wareflow.Application.Modules;
using Wareflow.Application.Tenancy;
using Wareflow.Domain.Modules;
using Wareflow.Infrastructure.Persistence;

namespace Wareflow.Infrastructure.Modules;

public sealed class TenantModuleService(
    WareflowDbContext dbContext,
    ITenantContext tenantContext,
    IModuleCatalog catalog,
    IPermissionChecker permissionChecker,
    IAuditLogWriter auditLogWriter,
    TimeProvider timeProvider) : IModuleService
{
    public async Task<IReadOnlyCollection<TenantModuleSnapshot>> GetEffectiveModulesAsync(
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        if (!tenantId.HasValue)
        {
            return [];
        }

        var permissions = await permissionChecker.GetPermissionsAsync(cancellationToken);
        await using var transaction = await BeginTenantTransactionAsync(
            tenantId.Value,
            cancellationToken);
        var settings = await dbContext.TenantModuleSettings
            .AsNoTracking()
            .ToDictionaryAsync(setting => setting.ModuleKey, StringComparer.Ordinal, cancellationToken);
        var entitlements = await dbContext.TenantSubscriptionEntitlements
            .AsNoTracking()
            .ToDictionaryAsync(entitlement => entitlement.ModuleKey, StringComparer.Ordinal, cancellationToken);
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
        return catalog.Modules
            .Select(module =>
            {
                var settingEnabled = settings.TryGetValue(module.Key, out var setting) && setting.Enabled;
                var entitled = entitledModuleKeys.Contains(module.Key);
                var permissionGranted = module.RequiredPermissions.IsSubsetOf(permissions);
                var dependenciesSatisfied = ModuleAccessEvaluator.DependenciesSatisfied(
                    catalog,
                    module,
                    enabledModuleKeys,
                    entitledModuleKeys);
                var effective = ModuleAccessEvaluator.HasAccess(
                    catalog,
                    module.Key,
                    enabledModuleKeys,
                    entitledModuleKeys,
                    permissions);
                return new TenantModuleSnapshot(
                    module.Key,
                    module.DisplayName,
                    settingEnabled,
                    entitled,
                    dependenciesSatisfied,
                    permissionGranted,
                    effective,
                    GetReason(module, settingEnabled, entitled, dependenciesSatisfied, permissionGranted));
            })
            .ToArray();
    }

    public async Task<bool> SetModuleEnabledAsync(
        string moduleKey,
        bool enabled,
        CancellationToken cancellationToken)
    {
        var module = catalog.FindModule(moduleKey);
        if (module is null ||
            !await permissionChecker.HasPermissionAsync(
                Permissions.ModulesManage,
                cancellationToken))
        {
            return false;
        }

        var tenantId = tenantContext.RequireTenantId();
        await using var transaction = await BeginTenantTransactionAsync(
            tenantId,
            cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var entitlements = await dbContext.TenantSubscriptionEntitlements
            .ToDictionaryAsync(item => item.ModuleKey, StringComparer.Ordinal, cancellationToken);
        var settings = await dbContext.TenantModuleSettings
            .ToDictionaryAsync(item => item.ModuleKey, StringComparer.Ordinal, cancellationToken);
        var existing = settings.TryGetValue(module.Key, out var current)
            ? current
            : null;
        var enabledModuleKeys = settings.Values
            .Where(setting => setting.Enabled)
            .Select(setting => setting.ModuleKey)
            .ToHashSet(StringComparer.Ordinal);
        var entitledModuleKeys = entitlements.Values
            .Where(entitlement => entitlement.IsValid(now))
            .Select(entitlement => entitlement.ModuleKey)
            .ToHashSet(StringComparer.Ordinal);

        if (enabled &&
            (module.Availability != ModuleAvailability.Available ||
             !entitledModuleKeys.Contains(module.Key) ||
             !ModuleAccessEvaluator.DependenciesSatisfied(
                catalog,
                module,
                enabledModuleKeys,
                entitledModuleKeys)))
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        if (!enabled &&
            catalog.Modules.Any(candidate =>
                candidate.Dependencies.Contains(module.Key) &&
                settings.TryGetValue(candidate.Key, out var dependentSetting) &&
                dependentSetting.Enabled))
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        if (existing is null)
        {
            dbContext.TenantModuleSettings.Add(new TenantModuleSetting(
                tenantId,
                module.Key,
                enabled,
                now));
        }
        else
        {
            existing.SetEnabled(enabled, now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogWriter.WriteAsync(
            enabled ? "TenantModuleEnabled" : "TenantModuleDisabled",
            nameof(TenantModuleSetting),
            module.Key,
            $$"""{"moduleKey":"{{module.Key}}","enabled":{{enabled.ToString().ToLowerInvariant()}}}""",
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<TenantSubscriptionSnapshot?> AssignPlanAsync(
        AssignSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        if (!await permissionChecker.HasPermissionAsync(
                Permissions.BillingManage,
                cancellationToken))
        {
            return null;
        }

        var plan = catalog.FindPlan(request.PlanKey);
        if (plan is null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = request.ExpiresAt?.UtcDateTime;
        if (expiresAt.HasValue && expiresAt.Value <= now)
        {
            throw new InvalidOperationException("The subscription expiration must be in the future.");
        }

        var tenantId = tenantContext.RequireTenantId();
        await using var transaction = await BeginTenantTransactionAsync(
            tenantId,
            cancellationToken);
        var entitlements = await dbContext.TenantSubscriptionEntitlements
            .ToDictionaryAsync(item => item.ModuleKey, StringComparer.Ordinal, cancellationToken);
        foreach (var module in catalog.Modules)
        {
            if (entitlements.TryGetValue(module.Key, out var entitlement))
            {
                entitlement.UpdatePlan(
                    plan.Key,
                    plan.IncludedModuleKeys.Contains(module.Key),
                    expiresAt,
                    now);
            }
            else
            {
                dbContext.TenantSubscriptionEntitlements.Add(new TenantSubscriptionEntitlement(
                    tenantId,
                    plan.Key,
                    module.Key,
                    plan.IncludedModuleKeys.Contains(module.Key),
                    expiresAt,
                    now));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogWriter.WriteAsync(
            "TenantSubscriptionPlanAssigned",
            nameof(TenantSubscriptionEntitlement),
            plan.Key,
            $$"""{"planKey":"{{plan.Key}}","expiresAt":"{{request.ExpiresAt?.ToString("O")}}"}""",
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TenantSubscriptionSnapshot(
            tenantId,
            plan.Key,
            request.ExpiresAt,
            plan.IncludedModuleKeys);
    }

    public async Task<TenantSubscriptionSnapshot?> GetSubscriptionAsync(
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        if (!tenantId.HasValue)
        {
            return null;
        }

        await using var transaction = await BeginTenantTransactionAsync(
            tenantId.Value,
            cancellationToken);
        var entitlements = await dbContext.TenantSubscriptionEntitlements
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        var first = entitlements.FirstOrDefault();
        return first is null
            ? null
            : new TenantSubscriptionSnapshot(
                tenantId.Value,
                first.PlanKey,
                first.ExpiresAtUtc.HasValue
                    ? new DateTimeOffset(first.ExpiresAtUtc.Value, TimeSpan.Zero)
                    : null,
                entitlements
                    .Where(entitlement => entitlement.Included)
                    .Select(entitlement => entitlement.ModuleKey)
                    .ToHashSet(StringComparer.Ordinal));
    }

    private static string? GetReason(
        ModuleDefinition module,
        bool settingEnabled,
        bool entitled,
        bool dependenciesSatisfied,
        bool permissionGranted) =>
        module.Availability != ModuleAvailability.Available ? "unavailable" :
        !entitled ? "not_entitled" :
        !settingEnabled ? "disabled" :
        !dependenciesSatisfied ? "dependency_disabled_or_unentitled" :
        !permissionGranted ? "missing_permission" :
        null;

    private async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>
        BeginTenantTransactionAsync(
            Guid tenantId,
            CancellationToken cancellationToken)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.tenant_id', {tenantId:N}, true)",
            cancellationToken);
        return transaction;
    }
}
