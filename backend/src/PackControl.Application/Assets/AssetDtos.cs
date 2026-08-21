namespace PackControl.Application.Assets;

public sealed record TechnicalAssetDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Code,
    string Alias,
    string AssetType,
    string Status,
    string Revision,
    IReadOnlyList<string> Components,
    IReadOnlyList<string> Materials,
    string? LastOrderNumber,
    string Notes,
    DateTime UpdatedAtUtc);
