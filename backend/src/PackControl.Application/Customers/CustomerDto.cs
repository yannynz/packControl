namespace PackControl.Application.Customers;

public sealed record CustomerProductPricingRuleDto(
    Guid ProductTemplateId,
    string ProductName,
    string BillingMethod,
    decimal UnitPrice,
    string? Notes);

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string? DocumentNumber,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Notes,
    IReadOnlyList<string> Nicknames,
    string? PostalCode,
    string? Street,
    string? StreetNumber,
    string? District,
    string? City,
    string? State,
    string? CityIbgeCode,
    string? StateRegistration,
    string TaxpayerIndicator,
    string? Complement,
    string? ReferencePoint,
    Guid? DefaultCarrierId,
    string? DefaultCarrierName,
    string? DefaultDeliveryMode,
    IReadOnlyList<CustomerProductPricingRuleDto> ProductPricingRules,
    int Score);
