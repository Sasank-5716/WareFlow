using Wareflow.Domain.Common;

namespace Wareflow.Domain.Modules;

public sealed class TenantModuleSetting : TenantOwnedEntity
{
    private TenantModuleSetting()
    {
    }

    public TenantModuleSetting(
        Guid tenantId,
        string moduleKey,
        bool enabled,
        DateTime createdAtUtc)
        : base(tenantId, createdAtUtc)
    {
        ModuleKey = RequireValue(moduleKey, nameof(moduleKey));
        Enabled = enabled;
    }

    public string ModuleKey { get; private set; } = null!;

    public bool Enabled { get; private set; }

    public void SetEnabled(bool enabled, DateTime updatedAtUtc)
    {
        Enabled = enabled;
        UpdatedAtUtc = DateTime.SpecifyKind(updatedAtUtc, DateTimeKind.Utc);
    }

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}
