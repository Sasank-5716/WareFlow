namespace Wareflow.Domain.Common;

public interface IHasUtcAuditMetadata
{
    DateTime CreatedAtUtc { get; }

    DateTime UpdatedAtUtc { get; }
}
