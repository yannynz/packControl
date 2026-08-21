using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Inventory;
using PackControl.Domain.Audit;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class InventoryService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor) : IInventoryService
{
    public async Task<IReadOnlyList<MaterialCardDto>> ListMaterialsAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return stateStore.Materials
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Name)
                .Select(x => new MaterialCardDto(
                    x.Id,
                    x.Name,
                    x.TechnicalType,
                    x.Category,
                    x.MainSupplier,
                    x.RiskLevel,
                    x.StandardCost,
                    x.LeadTimeDays,
                    x.Unit))
                .ToList();
        }
    }

    public async Task<IReadOnlyList<StockItemDto>> ListStockAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return MapStock(stateStore.StockItems);
        }
    }

    public async Task<IReadOnlyList<StockItemDto>> ReserveAsync(Guid stockItemId, decimal quantity, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            var item = stateStore.StockItems.SingleOrDefault(x => x.Id == stockItemId);
            if (item is not null)
            {
                var available = Math.Max(0, item.OnHand - item.Reserved);
                var delta = Math.Max(0, Math.Min(quantity, available));
                item.Reserved += delta;
                item.LastMovement = $"Reserva interna de {delta:0.##} un.";
                item.LastMovementAtUtc = clock.UtcNow;

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    "Stock",
                    item.Id,
                    "stock.reserved",
                    $"Reserva aplicada em {item.MaterialName}.",
                    JsonSerializer.Serialize(new { delta }),
                    clock.UtcNow));
            }

            return MapStock(stateStore.StockItems);
        }
    }

    public async Task<IReadOnlyList<StockItemDto>> ReplenishAsync(Guid stockItemId, decimal quantity, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            var item = stateStore.StockItems.SingleOrDefault(x => x.Id == stockItemId);
            if (item is not null)
            {
                var delta = Math.Max(0, quantity);
                item.OnHand += delta;
                item.LastMovement = $"Reposicao de {delta:0.##} un.";
                item.LastMovementAtUtc = clock.UtcNow;

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    "Stock",
                    item.Id,
                    "stock.replenished",
                    $"Reposicao aplicada em {item.MaterialName}.",
                    JsonSerializer.Serialize(new { delta }),
                    clock.UtcNow));
            }

            return MapStock(stateStore.StockItems);
        }
    }

    private static IReadOnlyList<StockItemDto> MapStock(IEnumerable<StockItemState> items)
    {
        return items
            .OrderBy(x => x.MaterialName)
            .Select(x =>
            {
                var available = x.OnHand - x.Reserved;
                var status = available <= x.ReorderPoint * 0.5m
                    ? "Critico"
                    : available <= x.ReorderPoint
                        ? "Baixo"
                        : "OK";

                return new StockItemDto(
                    x.Id,
                    x.MaterialId,
                    x.MaterialName,
                    x.OnHand,
                    x.Reserved,
                    available,
                    x.ReorderPoint,
                    status,
                    x.LastMovement,
                    x.LastMovementAtUtc);
            })
            .ToList();
    }
}
