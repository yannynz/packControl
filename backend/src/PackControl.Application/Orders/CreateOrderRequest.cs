using PackControl.Domain.Orders;

namespace PackControl.Application.Orders;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    ServiceType ServiceType,
    UrgencyLevel Urgency,
    string? ContextSummary,
    string? LegacyAssetReference,
    string? Notes,
    IReadOnlyList<ScopeItemRequest> ScopeItems);
