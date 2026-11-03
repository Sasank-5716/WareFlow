namespace Wareflow.Application.Modules;

public sealed record TenantModuleSnapshot(
    string ModuleKey,
    string DisplayName,
    bool Enabled,
    bool Entitled,
    bool DependenciesSatisfied,
    bool UserHasPermission,
    bool EffectiveAccess,
    string? Reason);

public sealed record UpdateTenantModuleRequest(bool Enabled);

public sealed record AssignSubscriptionPlanRequest(
    string PlanKey,
    DateTimeOffset? ExpiresAt);

public sealed record TenantSubscriptionSnapshot(
    Guid TenantId,
    string PlanKey,
    DateTimeOffset? ExpiresAt,
    IReadOnlySet<string> EntitledModuleKeys);

public sealed record ModuleCatalogSnapshot(
    IReadOnlyCollection<ModuleDefinition> Modules,
    IReadOnlyCollection<SubscriptionPlanDefinition> Plans);

public interface IModuleAccessChecker
{
    Task<bool> HasAccessAsync(
        string moduleKey,
        CancellationToken cancellationToken);
}

public interface IModuleService
{
    Task<IReadOnlyCollection<TenantModuleSnapshot>> GetEffectiveModulesAsync(
        CancellationToken cancellationToken);

    Task<bool> SetModuleEnabledAsync(
        string moduleKey,
        bool enabled,
        CancellationToken cancellationToken);

    Task<TenantSubscriptionSnapshot?> AssignPlanAsync(
        AssignSubscriptionPlanRequest request,
        CancellationToken cancellationToken);

    Task<TenantSubscriptionSnapshot?> GetSubscriptionAsync(
        CancellationToken cancellationToken);
}
