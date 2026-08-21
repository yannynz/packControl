namespace PackControl.Application.Logistics;

public sealed record ShipmentDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    string ShipmentNumber,
    string CustomerName,
    string Mode,
    string Status,
    string Recipient,
    Guid? CarrierId,
    string? CarrierName,
    string DriverName,
    string VehiclePlate,
    string ChecklistStatus,
    bool HasOccurrence,
    DateTime ScheduledAtUtc);

public sealed record LogisticsOverviewDto(
    int PendingShipments,
    int TodayShipments,
    int AdverseShipments,
    IReadOnlyList<ShipmentDto> Shipments);
