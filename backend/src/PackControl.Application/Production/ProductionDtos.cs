namespace PackControl.Application.Production;

public sealed record SectorQueueDto(
    string Name,
    int Pending,
    int InProgress,
    int Late,
    string DefaultOwner,
    int EfficiencyPercent);

public sealed record ProductionOrderCardDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    string Number,
    bool VisibleInQueues,
    Guid? ParentProductionOrderId,
    string? ParentProductionOrderNumber,
    Guid? MergedIntoProductionOrderId,
    string? MergedIntoProductionOrderNumber,
    IReadOnlyList<string> RelatedProductionOrderNumbers,
    string? TraceabilityReason,
    string CustomerName,
    string Title,
    int Quantity,
    Guid? ProductTemplateId,
    string? ProductName,
    string BillingMethod,
    decimal? UnitPrice,
    string Sector,
    string Status,
    string Priority,
    string Owner,
    int Complexity,
    bool Outsourced,
    string MaterialSupport,
    DateTime DueAtUtc,
    DateTime UpdatedAtUtc);

public sealed record ProductionSectorDetailDto(
    string Name,
    string DefaultOwner,
    int Pending,
    int InProgress,
    int Late,
    IReadOnlyList<ProductionOrderCardDto> Orders);

public sealed record ProductionOverviewDto(
    IReadOnlyList<SectorQueueDto> Sectors,
    IReadOnlyList<ProductionOrderCardDto> Orders);

public sealed record SplitProductionOrderPartRequest(
    string Title,
    int Quantity,
    string? Sector);

public sealed record SplitProductionOrderRequest(
    string? Reason,
    IReadOnlyList<SplitProductionOrderPartRequest> Parts);

public sealed record MergeProductionOrdersRequest(
    IReadOnlyList<Guid> ProductionOrderIds,
    string? Title,
    string? Sector,
    string? Reason);
