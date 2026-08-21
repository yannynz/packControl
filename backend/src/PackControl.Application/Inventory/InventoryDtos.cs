namespace PackControl.Application.Inventory;

public sealed record MaterialCardDto(
    Guid Id,
    string Name,
    string TechnicalType,
    string Category,
    string MainSupplier,
    string RiskLevel,
    decimal StandardCost,
    int LeadTimeDays,
    string Unit);

public sealed record StockItemDto(
    Guid Id,
    Guid MaterialId,
    string MaterialName,
    decimal OnHand,
    decimal Reserved,
    decimal Available,
    decimal ReorderPoint,
    string Status,
    string LastMovement,
    DateTime LastMovementAtUtc);
