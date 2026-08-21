using PackControl.Domain.Common;

namespace PackControl.Domain.Orders;

public sealed class TechnicalAnalysis : AuditableEntity
{
    private TechnicalAnalysis()
    {
    }

    public Guid OrderId { get; private set; }
    public Guid AttachmentId { get; private set; }
    public string SourceFileExtension { get; private set; } = string.Empty;
    public TechnicalAnalysisStatus Status { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string? EngineName { get; private set; }
    public int? ConfidencePercent { get; private set; }

    internal static TechnicalAnalysis CreatePending(
        Guid orderId,
        Guid attachmentId,
        string sourceFileExtension,
        string summary,
        DateTime utcNow,
        string actor)
    {
        var analysis = new TechnicalAnalysis
        {
            OrderId = orderId,
            AttachmentId = attachmentId,
            SourceFileExtension = sourceFileExtension.Trim().ToLowerInvariant(),
            Status = TechnicalAnalysisStatus.PendingEngine,
            Summary = summary.Trim()
        };

        analysis.MarkCreated(utcNow, actor);
        return analysis;
    }

    public static TechnicalAnalysis Restore(
        Guid id,
        Guid orderId,
        Guid attachmentId,
        string sourceFileExtension,
        TechnicalAnalysisStatus status,
        string summary,
        string? engineName,
        int? confidencePercent,
        DateTime createdAtUtc,
        string createdBy,
        DateTime? updatedAtUtc,
        string? updatedBy)
    {
        return new TechnicalAnalysis
        {
            Id = id,
            OrderId = orderId,
            AttachmentId = attachmentId,
            SourceFileExtension = sourceFileExtension,
            Status = status,
            Summary = summary,
            EngineName = engineName,
            ConfidencePercent = confidencePercent,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = updatedAtUtc,
            UpdatedBy = updatedBy
        };
    }

    public void Complete(string summary, string engineName, int confidencePercent, DateTime utcNow, string actor)
    {
        Status = TechnicalAnalysisStatus.Completed;
        Summary = summary.Trim();
        EngineName = string.IsNullOrWhiteSpace(engineName) ? null : engineName.Trim();
        ConfidencePercent = Math.Clamp(confidencePercent, 0, 100);
        MarkUpdated(utcNow, actor);
    }

    public void Fail(string summary, string engineName, DateTime utcNow, string actor)
    {
        Status = TechnicalAnalysisStatus.Failed;
        Summary = summary.Trim();
        EngineName = string.IsNullOrWhiteSpace(engineName) ? null : engineName.Trim();
        ConfidencePercent = 0;
        MarkUpdated(utcNow, actor);
    }
}
