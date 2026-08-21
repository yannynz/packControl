namespace PackControl.Application.Carriers;

public sealed record CarrierDto(
    Guid Id,
    string Name,
    string ContactName,
    string Email,
    string Phone,
    string BusinessHours,
    string ServiceArea,
    string DefaultMode,
    bool DoesPickup,
    bool DoesDelivery,
    string Notes,
    DateTime UpdatedAtUtc);
