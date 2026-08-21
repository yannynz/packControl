using PackControl.Domain.Common;

namespace PackControl.Domain.Orders;

public sealed class Order : AuditableEntity
{
    private Order()
    {
    }

    public string Number { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public ServiceType ServiceType { get; private set; }
    public UrgencyLevel Urgency { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? ContextSummary { get; private set; }
    public string? LegacyAssetReference { get; private set; }
    public string? Notes { get; private set; }
    public List<OrderScopeItem> ScopeItems { get; private set; } = [];
    public List<OrderAttachment> Attachments { get; private set; } = [];
    public List<TechnicalAnalysis> Analyses { get; private set; } = [];

    public static Order Create(
        string number,
        Guid customerId,
        ServiceType serviceType,
        UrgencyLevel urgency,
        string? contextSummary,
        string? legacyAssetReference,
        string? notes,
        DateTime utcNow,
        string actor)
    {
        var order = new Order
        {
            Number = number,
            CustomerId = customerId,
            ServiceType = serviceType,
            Urgency = urgency,
            Status = OrderStatus.Draft,
            ContextSummary = string.IsNullOrWhiteSpace(contextSummary) ? null : contextSummary.Trim(),
            LegacyAssetReference = string.IsNullOrWhiteSpace(legacyAssetReference) ? null : legacyAssetReference.Trim(),
            Notes = notes?.Trim()
        };

        order.MarkCreated(utcNow, actor);
        return order;
    }

    public static Order Restore(
        Guid id,
        string number,
        Guid customerId,
        ServiceType serviceType,
        UrgencyLevel urgency,
        OrderStatus status,
        string? contextSummary,
        string? legacyAssetReference,
        string? notes,
        IEnumerable<OrderScopeItem>? scopeItems,
        IEnumerable<OrderAttachment>? attachments,
        IEnumerable<TechnicalAnalysis>? analyses,
        DateTime createdAtUtc,
        string createdBy,
        DateTime? updatedAtUtc,
        string? updatedBy)
    {
        return new Order
        {
            Id = id,
            Number = number,
            CustomerId = customerId,
            ServiceType = serviceType,
            Urgency = urgency,
            Status = status,
            ContextSummary = contextSummary,
            LegacyAssetReference = legacyAssetReference,
            Notes = notes,
            ScopeItems = scopeItems?.ToList() ?? [],
            Attachments = attachments?.ToList() ?? [],
            Analyses = analyses?.ToList() ?? [],
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = updatedAtUtc,
            UpdatedBy = updatedBy
        };
    }

    public void AddScopeItem(
        string title,
        string category,
        int quantity,
        Guid? productTemplateId,
        string? productName,
        string? billingMethod,
        decimal? unitPrice,
        string? notes,
        DateTime utcNow,
        string actor)
    {
        ScopeItems.Add(OrderScopeItem.Create(
            Id,
            title,
            category,
            quantity,
            productTemplateId,
            productName,
            billingMethod,
            unitPrice,
            notes,
            utcNow,
            actor));
        MarkUpdated(utcNow, actor);
    }

    public OrderAttachment AddAttachment(
        string originalFileName,
        string storedFileName,
        string storagePath,
        string contentType,
        long sizeBytes,
        string sha256,
        DateTime utcNow,
        string actor)
    {
        var attachment = OrderAttachment.Create(
            Id,
            originalFileName,
            storedFileName,
            storagePath,
            contentType,
            sizeBytes,
            sha256,
            utcNow,
            actor);

        Attachments.Add(attachment);
        MarkUpdated(utcNow, actor);
        return attachment;
    }

    public TechnicalAnalysis RegisterPendingAnalysis(Guid attachmentId, string extension, string summary, DateTime utcNow, string actor)
    {
        var analysis = TechnicalAnalysis.CreatePending(Id, attachmentId, extension, summary, utcNow, actor);
        Analyses.Add(analysis);
        Status = OrderStatus.AwaitingTechnicalAnalysis;
        MarkUpdated(utcNow, actor);
        return analysis;
    }

    public void Approve(DateTime utcNow, string actor)
    {
        Status = OrderStatus.Approved;
        MarkUpdated(utcNow, actor);
    }

    public void MarkInProduction(DateTime utcNow, string actor)
    {
        Status = OrderStatus.InProduction;
        MarkUpdated(utcNow, actor);
    }
}
