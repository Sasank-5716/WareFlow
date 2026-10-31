namespace Wareflow.Application.Persistence;

public interface ITenantTransactionManager
{
    Task<ITenantTransaction> BeginAsync(CancellationToken cancellationToken = default);
}

public interface ITenantTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
