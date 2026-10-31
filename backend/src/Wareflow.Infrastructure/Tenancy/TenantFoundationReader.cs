using Microsoft.EntityFrameworkCore;
using Wareflow.Application.Tenancy;
using Wareflow.Infrastructure.Persistence;

namespace Wareflow.Infrastructure.Tenancy;

public sealed class TenantFoundationReader(
    WareflowDbContext dbContext) : ITenantFoundationReader
{
    public async Task<TenantSettingsSnapshot?> ReadSettingsAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.TenantSettings
            .AsNoTracking()
            .Select(settings => new TenantSettingsSnapshot(
                settings.TenantId,
                settings.TimeZoneId,
                settings.Locale,
                settings.DefaultCurrencyCode))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
