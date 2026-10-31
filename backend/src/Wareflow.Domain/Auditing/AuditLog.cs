using Wareflow.Domain.Common;

namespace Wareflow.Domain.Auditing;

public sealed class AuditLog : TenantOwnedEntity
{
    private AuditLog()
    {
    }

    public AuditLog(
        Guid tenantId,
        string action,
        string entityType,
        string? entityId,
        string? actorIdentityUserId,
        string? correlationId,
        DateTime occurredAtUtc,
        string? safeMetadataJson = null)
        : base(tenantId, occurredAtUtc)
    {
        Action = RequireValue(action, nameof(action));
        EntityType = RequireValue(entityType, nameof(entityType));
        EntityId = entityId;
        ActorIdentityUserId = actorIdentityUserId;
        CorrelationId = correlationId;
        OccurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc);
        SafeMetadataJson = safeMetadataJson;
    }

    public string Action { get; private set; } = null!;

    public string EntityType { get; private set; } = null!;

    public string? EntityId { get; private set; }

    public string? ActorIdentityUserId { get; private set; }

    public string? CorrelationId { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public string? SafeMetadataJson { get; private set; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }
}
