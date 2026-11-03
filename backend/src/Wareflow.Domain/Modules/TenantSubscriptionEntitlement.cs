using Wareflow.Domain.Common;

namespace Wareflow.Domain.Modules;

public sealed class TenantSubscriptionEntitlement : TenantOwnedEntity
{
    private TenantSubscriptionEntitlement()
    {
    }

    public TenantSubscriptionEntitlement(
        Guid tenantId,
        string planKey,
        string moduleKey,
        bool included,
        DateTime? expiresAtUtc,
        DateTime createdAtUtc)
        : base(tenantId, createdAtUtc)
    {
        PlanKey = RequireValue(planKey, nameof(planKey));
        ModuleKey = RequireValue(moduleKey, nameof(moduleKey));
        Included = included;
        ExpiresAtUtc = expiresAtUtc.HasValue
            ? DateTime.SpecifyKind(expiresAtUtc.Value, DateTimeKind.Utc)
            : null;
    }

    public string PlanKey { get; private set; } = null!;

    public string ModuleKey { get; private set; } = null!;

    public bool Included { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public bool IsValid(DateTime utcNow) =>
        Included && (!ExpiresAtUtc.HasValue || ExpiresAtUtc.Value > utcNow);

    public void UpdatePlan(
        string planKey,
        bool included,
        DateTime? expiresAtUtc,
        DateTime updatedAtUtc)
    {
        PlanKey = RequireValue(planKey, nameof(planKey));
        Included = included;
        ExpiresAtUtc = expiresAtUtc.HasValue
            ? DateTime.SpecifyKind(expiresAtUtc.Value, DateTimeKind.Utc)
            : null;
        UpdatedAtUtc = DateTime.SpecifyKind(updatedAtUtc, DateTimeKind.Utc);
    }

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}
