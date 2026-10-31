using Microsoft.EntityFrameworkCore;
using Wareflow.Application.Persistence;
using Wareflow.Application.Tenancy;
using Wareflow.Infrastructure.Persistence;

namespace Wareflow.Infrastructure.Tenancy;

public sealed class TenantTransactionManager(
    WareflowDbContext dbContext,
    ITenantContext tenantContext) : ITenantTransactionManager
{
    public async Task<ITenantTransaction> BeginAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.RequireTenantId();
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.tenant_id', {tenantId.ToString()}, true)",
            cancellationToken);

        return new TenantTransaction(transaction);
    }

    private sealed class TenantTransaction(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
        : ITenantTransaction
    {
        private bool _completed;

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await transaction.CommitAsync(cancellationToken);
            _completed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_completed)
            {
                await transaction.RollbackAsync();
            }

            await transaction.DisposeAsync();
        }
    }
}
