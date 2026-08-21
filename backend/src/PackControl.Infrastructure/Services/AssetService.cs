using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Assets;
using PackControl.Domain.Audit;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class AssetService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IAppStatePersistence statePersistence) : IAssetService
{
    public async Task<IReadOnlyList<TechnicalAssetDto>> ListAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return stateStore.TechnicalAssets
                .OrderBy(x => x.CustomerName)
                .ThenBy(x => x.Code)
                .Select(Map)
                .ToList();
        }
    }

    public async Task<TechnicalAssetDto> CreateAsync(CreateTechnicalAssetRequest request, CancellationToken cancellationToken)
    {
        TechnicalAssetDto assetDto;
        lock (stateStore.SyncRoot)
        {
            var customer = stateStore.Customers.SingleOrDefault(x => x.Id == request.CustomerId)
                ?? throw new InvalidOperationException("Cliente do ativo nao encontrado.");

            var asset = new TechnicalAssetState
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                Code = request.Code.Trim(),
                Alias = request.Alias.Trim(),
                AssetType = request.AssetType.Trim(),
                Status = request.Status.Trim(),
                Revision = request.Revision.Trim(),
                Components = NormalizeList(request.Components),
                Materials = NormalizeList(request.Materials),
                LastOrderNumber = Normalize(request.LastOrderNumber),
                Notes = request.Notes.Trim(),
                UpdatedAtUtc = clock.UtcNow
            };

            stateStore.TechnicalAssets.Add(asset);
            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(TechnicalAssetState),
                asset.Id,
                "asset.created",
                $"Ativo tecnico {asset.Code} criado.",
                JsonSerializer.Serialize(new { asset.CustomerName, asset.AssetType }),
                clock.UtcNow));

            assetDto = Map(asset);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return assetDto;
    }

    public async Task<TechnicalAssetDto?> UpdateAsync(
        Guid assetId,
        UpdateTechnicalAssetRequest request,
        CancellationToken cancellationToken)
    {
        TechnicalAssetDto? assetDto;
        lock (stateStore.SyncRoot)
        {
            var asset = stateStore.TechnicalAssets.SingleOrDefault(x => x.Id == assetId);
            if (asset is null)
            {
                return null;
            }

            var customer = stateStore.Customers.SingleOrDefault(x => x.Id == request.CustomerId)
                ?? throw new InvalidOperationException("Cliente do ativo nao encontrado.");

            asset.CustomerId = customer.Id;
            asset.CustomerName = customer.Name;
            asset.Code = request.Code.Trim();
            asset.Alias = request.Alias.Trim();
            asset.AssetType = request.AssetType.Trim();
            asset.Status = request.Status.Trim();
            asset.Revision = request.Revision.Trim();
            asset.Components = NormalizeList(request.Components);
            asset.Materials = NormalizeList(request.Materials);
            asset.LastOrderNumber = Normalize(request.LastOrderNumber);
            asset.Notes = request.Notes.Trim();
            asset.UpdatedAtUtc = clock.UtcNow;

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(TechnicalAssetState),
                asset.Id,
                "asset.updated",
                $"Ativo tecnico {asset.Code} atualizado.",
                JsonSerializer.Serialize(new { asset.CustomerName, asset.AssetType, asset.Revision }),
                clock.UtcNow));

            assetDto = Map(asset);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return assetDto;
    }

    private static TechnicalAssetDto Map(TechnicalAssetState asset) => new(
        asset.Id,
        asset.CustomerId,
        asset.CustomerName,
        asset.Code,
        asset.Alias,
        asset.AssetType,
        asset.Status,
        asset.Revision,
        asset.Components,
        asset.Materials,
        asset.LastOrderNumber,
        asset.Notes,
        asset.UpdatedAtUtc);

    private static List<string> NormalizeList(IEnumerable<string> values) =>
        values
            .Select(Normalize)
            .OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
