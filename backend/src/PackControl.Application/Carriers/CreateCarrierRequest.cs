namespace PackControl.Application.Carriers;

public sealed record CreateCarrierRequest(
    string Name,
    string ContactName,
    string Email,
    string Phone,
    string BusinessHours,
    string ServiceArea,
    string DefaultMode,
    bool DoesPickup,
    bool DoesDelivery,
    string Notes);
