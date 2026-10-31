using Wareflow.Application.Persistence;

namespace Wareflow.Application.Tenancy;

public sealed class TenantFoundationStore(
    ITenantContext tenantContext,
    ITenantTransactionManager transactionManager,
    ITenantFoundationReader reader) : ITenantFoundationStore
{
    public async Task<TenantSettingsSnapshot?> GetSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        tenantContext.RequireTenantId();

        await using var transaction =
            await transactionManager.BeginAsync(cancellationToken);

        var settings = await reader.ReadSettingsAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return settings;
    }
}

public interface ITenantFoundationReader
{
    Task<TenantSettingsSnapshot?> ReadSettingsAsync(CancellationToken cancellationToken);
}
