namespace PackControl.Application.Orders;

public sealed record ScopeItemRequest(
    string Title,
    string Category,
    int Quantity,
    Guid? ProductTemplateId,
    string? ProductName,
    string? BillingMethod,
    decimal? UnitPrice,
    string? Notes);
