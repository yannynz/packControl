using PackControl.Domain.Common;

namespace PackControl.Domain.Audit;

public sealed class AuditLog : Entity
{
    private AuditLog()
    {
    }

    public Guid? ActorUserId { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? MetadataJson { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    public static AuditLog Create(
        Guid? actorUserId,
        string actorName,
        string entityName,
        Guid entityId,
        string action,
        string description,
        string? metadataJson,
        DateTime occurredAtUtc)
    {
        return new AuditLog
        {
            ActorUserId = actorUserId,
            ActorName = actorName,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            Description = description,
            MetadataJson = metadataJson,
            OccurredAtUtc = occurredAtUtc
        };
    }

    public static AuditLog Restore(
        Guid id,
        Guid? actorUserId,
        string actorName,
        string entityName,
        Guid entityId,
        string action,
        string description,
        string? metadataJson,
        DateTime occurredAtUtc)
    {
        return new AuditLog
        {
            Id = id,
            ActorUserId = actorUserId,
            ActorName = actorName,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            Description = description,
            MetadataJson = metadataJson,
            OccurredAtUtc = occurredAtUtc
        };
    }
}
