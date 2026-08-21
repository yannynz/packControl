namespace PackControl.Application.Assets;

public interface IAssetService
{
    Task<IReadOnlyList<TechnicalAssetDto>> ListAsync(CancellationToken cancellationToken);
    Task<TechnicalAssetDto> CreateAsync(CreateTechnicalAssetRequest request, CancellationToken cancellationToken);
    Task<TechnicalAssetDto?> UpdateAsync(Guid assetId, UpdateTechnicalAssetRequest request, CancellationToken cancellationToken);
}
