namespace PackControl.Application.Assets;

public sealed record CreateTechnicalAssetRequest(
    Guid CustomerId,
    string Code,
    string Alias,
    string AssetType,
    string Status,
    string Revision,
    IReadOnlyList<string> Components,
    IReadOnlyList<string> Materials,
    string? LastOrderNumber,
    string Notes);
