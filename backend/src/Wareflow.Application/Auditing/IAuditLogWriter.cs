namespace Wareflow.Application.Auditing;

public interface IAuditLogWriter
{
    Task WriteAsync(
        string action,
        string entityType,
        string? entityId,
        string? safeMetadataJson,
        CancellationToken cancellationToken);
}
