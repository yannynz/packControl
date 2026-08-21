namespace PackControl.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; protected set; }
    public string CreatedBy { get; protected set; } = "system";
    public DateTime? UpdatedAtUtc { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    protected void MarkCreated(DateTime utcNow, string actor)
    {
        CreatedAtUtc = utcNow;
        CreatedBy = actor;
        UpdatedAtUtc = utcNow;
        UpdatedBy = actor;
    }

    protected void MarkUpdated(DateTime utcNow, string actor)
    {
        UpdatedAtUtc = utcNow;
        UpdatedBy = actor;
    }
}
