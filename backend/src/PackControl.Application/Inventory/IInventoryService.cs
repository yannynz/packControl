namespace PackControl.Application.Inventory;

public interface IInventoryService
{
    Task<IReadOnlyList<MaterialCardDto>> ListMaterialsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<StockItemDto>> ListStockAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<StockItemDto>> ReserveAsync(Guid stockItemId, decimal quantity, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockItemDto>> ReplenishAsync(Guid stockItemId, decimal quantity, CancellationToken cancellationToken);
}
