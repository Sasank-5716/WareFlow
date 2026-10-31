namespace Wareflow.Application.Tenancy;

public interface ITenantFoundationStore
{
    Task<TenantSettingsSnapshot?> GetSettingsAsync(CancellationToken cancellationToken = default);
}

public sealed record TenantSettingsSnapshot(
    Guid TenantId,
    string TimeZoneId,
    string Locale,
    string DefaultCurrencyCode);
