namespace PackControl.Application.Orders;

public sealed record OrderListItemDto(
    Guid Id,
    string Number,
    string CustomerName,
    string Status,
    string ServiceType,
    string Urgency,
    string ScopePreview,
    int ScopeItemsCount,
    int AttachmentsCount,
    DateTime CreatedAtUtc);

public sealed record OrderScopeItemDto(
    Guid Id,
    string Title,
    string Category,
    int Quantity,
    Guid? ProductTemplateId,
    string? ProductName,
    string? BillingMethod,
    decimal? UnitPrice,
    string? Notes);

public sealed record OrderAttachmentDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    DateTime UploadedAtUtc);

public sealed record TechnicalAnalysisDto(
    Guid Id,
    Guid AttachmentId,
    string SourceFileExtension,
    string Status,
    string Summary,
    string? EngineName,
    int? ConfidencePercent,
    DateTime CreatedAtUtc);

public sealed record RelatedProductionOrderDto(
    Guid Id,
    string Number,
    string Sector,
    string Status,
    string Owner,
    DateTime DueAtUtc);

public sealed record RelatedShipmentDto(
    Guid Id,
    string ShipmentNumber,
    string Mode,
    string Status,
    string Recipient,
    string? CarrierName,
    DateTime ScheduledAtUtc);

public sealed record RelatedFinanceEntryDto(
    Guid Id,
    string Type,
    string Status,
    string Description,
    decimal Amount,
    string EntrySource,
    string PaymentMethod,
    string? BoletoStatus,
    string? BoletoNumber,
    DateTime DueAtUtc);

public sealed record OrderHistoryEntryDto(
    Guid Id,
    string EventType,
    string Description,
    string Actor,
    DateTime OccurredAtUtc);

public sealed record OrderDetailDto(
    Guid Id,
    string Number,
    Guid CustomerId,
    string CustomerName,
    string Status,
    string ServiceType,
    string Urgency,
    string? ContextSummary,
    string? LegacyAssetReference,
    string? Notes,
    IReadOnlyList<OrderScopeItemDto> ScopeItems,
    IReadOnlyList<OrderAttachmentDto> Attachments,
    IReadOnlyList<TechnicalAnalysisDto> Analyses,
    IReadOnlyList<RelatedProductionOrderDto> ProductionOrders,
    IReadOnlyList<RelatedShipmentDto> Shipments,
    IReadOnlyList<RelatedFinanceEntryDto> FinanceEntries,
    IReadOnlyList<OrderHistoryEntryDto> History,
    DateTime CreatedAtUtc);
