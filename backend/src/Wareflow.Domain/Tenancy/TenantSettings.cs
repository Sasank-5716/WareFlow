using Wareflow.Domain.Common;

namespace Wareflow.Domain.Tenancy;

public sealed class TenantSettings : TenantOwnedEntity
{
    private TenantSettings()
    {
    }

    public TenantSettings(
        Guid tenantId,
        string timeZoneId,
        string locale,
        string defaultCurrencyCode,
        DateTime createdAtUtc)
        : base(tenantId, createdAtUtc)
    {
        TimeZoneId = RequireValue(timeZoneId, nameof(timeZoneId));
        Locale = RequireValue(locale, nameof(locale));
        DefaultCurrencyCode = RequireValue(defaultCurrencyCode, nameof(defaultCurrencyCode));
    }

    public string TimeZoneId { get; private set; } = null!;

    public string Locale { get; private set; } = null!;

    public string DefaultCurrencyCode { get; private set; } = null!;

    public void Update(
        string timeZoneId,
        string locale,
        string defaultCurrencyCode,
        DateTime updatedAtUtc)
    {
        TimeZoneId = RequireValue(timeZoneId, nameof(timeZoneId));
        Locale = RequireValue(locale, nameof(locale));
        DefaultCurrencyCode = RequireValue(defaultCurrencyCode, nameof(defaultCurrencyCode));
        UpdatedAtUtc = DateTime.SpecifyKind(updatedAtUtc, DateTimeKind.Utc);
    }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }
}
