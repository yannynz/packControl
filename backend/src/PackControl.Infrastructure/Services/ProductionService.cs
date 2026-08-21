using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Production;
using PackControl.Domain.Audit;
using PackControl.Domain.Orders;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class ProductionService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IAppStatePersistence statePersistence) : IProductionService
{
    private static readonly string[] SectorFlow = ["Preparacao", "Corte", "Montagem", "Emborrachamento", "Expedicao"];

    public async Task<ProductionOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return MapOverview(stateStore, clock.UtcNow);
        }
    }

    public async Task<ProductionSectorDetailDto> GetSectorAsync(string sector, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return MapSector(stateStore, clock.UtcNow, sector);
        }
    }

    public async Task<ProductionOverviewDto> AdvanceAsync(Guid productionOrderId, CancellationToken cancellationToken)
    {
        ProductionOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            var item = stateStore.ProductionOrders.SingleOrDefault(x => x.Id == productionOrderId);
            if (item is null)
            {
                return MapOverview(stateStore, clock.UtcNow);
            }

            if (!item.VisibleInQueues)
            {
                return MapOverview(stateStore, clock.UtcNow);
            }

            var order = stateStore.Orders.SingleOrDefault(x => x.Id == item.OrderId);
            if (order is null)
            {
                return MapOverview(stateStore, clock.UtcNow);
            }

            if (item.Status == "Aguardando fila")
            {
                item.Status = "Em producao";
                item.UpdatedAtUtc = clock.UtcNow;
                order.MarkInProduction(clock.UtcNow, currentUserAccessor.DisplayName);
            }
            else if (item.Status == "Em producao")
            {
                var nextSector = ResolveNextSector(item.Sector);
                if (nextSector is null)
                {
                    item.Status = "Concluida";
                }
                else
                {
                    item.Sector = nextSector;
                    item.Status = "Aguardando fila";
                    item.Owner = ResolveSectorOwner(nextSector);
                }

                item.UpdatedAtUtc = clock.UtcNow;
            }

            if (stateStore.ProductionOrders.Where(x => x.OrderId == item.OrderId).All(x => x.Status == "Concluida"))
            {
                var shipment = stateStore.Shipments.SingleOrDefault(x => x.OrderId == item.OrderId);
                if (shipment is not null)
                {
                    shipment.Status = "Pronto para expedir";
                    shipment.ChecklistStatus = "Liberado";
                }
            }

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(Order),
                order.Id,
                "production.order_advanced",
                $"OP {item.Number} avancada para {item.Sector} ({item.Status}).",
                JsonSerializer.Serialize(new { item.Number, item.Sector, item.Status }),
                clock.UtcNow));

            overview = MapOverview(stateStore, clock.UtcNow);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    public async Task<ProductionOverviewDto> SplitAsync(Guid productionOrderId, SplitProductionOrderRequest request, CancellationToken cancellationToken)
    {
        ProductionOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            var source = stateStore.ProductionOrders.SingleOrDefault(x => x.Id == productionOrderId);
            if (source is null || !source.VisibleInQueues)
            {
                return MapOverview(stateStore, clock.UtcNow);
            }

            var parts = request.Parts
                .Where(x => x.Quantity > 0)
                .ToList();

            if (parts.Count < 2)
            {
                throw new InvalidOperationException("O split precisa de pelo menos duas partes validas.");
            }

            var totalQuantity = parts.Sum(x => x.Quantity);
            if (totalQuantity != source.Quantity)
            {
                throw new InvalidOperationException("A soma do split deve bater exatamente com a quantidade da OP.");
            }

            var order = stateStore.Orders.SingleOrDefault(x => x.Id == source.OrderId);
            if (order is null)
            {
                return MapOverview(stateStore, clock.UtcNow);
            }

            source.VisibleInQueues = false;
            source.Status = "Desmembrada";
            source.TraceabilityReason = request.Reason?.Trim();
            source.UpdatedAtUtc = clock.UtcNow;

            var splitIndex = stateStore.ProductionOrders.Count(x => x.OrderId == source.OrderId && x.Number.Contains("-S", StringComparison.OrdinalIgnoreCase));
            for (var index = 0; index < parts.Count; index++)
            {
                var part = parts[index];
                var sector = NormalizeSector(part.Sector) ?? source.Sector;
                var number = $"{source.Number}-S{splitIndex + index + 1:00}";

                stateStore.ProductionOrders.Add(CloneOrder(
                    source,
                    Guid.NewGuid(),
                    number,
                    part.Title,
                    part.Quantity,
                    sector,
                    ResolveSectorOwner(sector),
                    source.Priority,
                    source.DueAtUtc,
                    source.Id,
                    source.Number,
                    null,
                    null,
                    [source.Number],
                    request.Reason));
            }

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(Order),
                order.Id,
                "production.order_split",
                $"OP {source.Number} desmembrada em {parts.Count} partes.",
                JsonSerializer.Serialize(new
                {
                    source.Number,
                    Parts = parts.Select(x => new { x.Title, x.Quantity, x.Sector }),
                    request.Reason
                }),
                clock.UtcNow));

            overview = MapOverview(stateStore, clock.UtcNow);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    public async Task<ProductionOverviewDto> MergeAsync(MergeProductionOrdersRequest request, CancellationToken cancellationToken)
    {
        ProductionOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            var ids = request.ProductionOrderIds.Distinct().ToList();
            if (ids.Count < 2)
            {
                throw new InvalidOperationException("Selecione pelo menos duas OPs para mesclar.");
            }

            var selectedOrders = stateStore.ProductionOrders
                .Where(x => ids.Contains(x.Id) && x.VisibleInQueues)
                .OrderBy(x => x.Number)
                .ToList();

            if (selectedOrders.Count != ids.Count)
            {
                throw new InvalidOperationException("Uma ou mais OPs selecionadas nao estao disponiveis para merge.");
            }

            if (selectedOrders.Select(x => x.OrderId).Distinct().Count() != 1)
            {
                throw new InvalidOperationException("O merge precisa ocorrer dentro do mesmo pedido.");
            }

            var prototype = selectedOrders.First();
            var order = stateStore.Orders.SingleOrDefault(x => x.Id == prototype.OrderId);
            if (order is null)
            {
                return MapOverview(stateStore, clock.UtcNow);
            }

            var targetId = Guid.NewGuid();
            var mergedIndex = stateStore.ProductionOrders.Count(x => x.OrderId == prototype.OrderId && x.Number.Contains("-M", StringComparison.OrdinalIgnoreCase));
            var targetNumber = $"OP-{prototype.OrderNumber}-M{mergedIndex + 1:00}";
            var sector = NormalizeSector(request.Sector) ?? prototype.Sector;
            var reason = request.Reason?.Trim();
            var relatedNumbers = selectedOrders.Select(x => x.Number).ToList();

            foreach (var item in selectedOrders)
            {
                item.VisibleInQueues = false;
                item.Status = "Mesclada";
                item.MergedIntoProductionOrderId = targetId;
                item.MergedIntoProductionOrderNumber = targetNumber;
                item.TraceabilityReason = reason;
                item.UpdatedAtUtc = clock.UtcNow;
            }

            stateStore.ProductionOrders.Add(CloneOrder(
                prototype,
                targetId,
                targetNumber,
                string.IsNullOrWhiteSpace(request.Title) ? $"Merge de {prototype.Title}" : request.Title.Trim(),
                selectedOrders.Sum(x => x.Quantity),
                sector,
                ResolveSectorOwner(sector),
                prototype.Priority,
                selectedOrders.Max(x => x.DueAtUtc),
                null,
                null,
                null,
                null,
                relatedNumbers,
                reason));

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(Order),
                order.Id,
                "production.order_merged",
                $"OPs {string.Join(", ", relatedNumbers)} mescladas em {targetNumber}.",
                JsonSerializer.Serialize(new { relatedNumbers, targetNumber, request.Title, request.Sector, request.Reason }),
                clock.UtcNow));

            overview = MapOverview(stateStore, clock.UtcNow);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    private static ProductionOverviewDto MapOverview(AppStateStore stateStore, DateTime utcNow)
    {
        var sectors = SectorFlow
            .Select(sector =>
            {
                var rows = stateStore.ProductionOrders.Where(x => x.Sector == sector).ToList();
                rows = rows.Where(x => x.VisibleInQueues).ToList();
                var pending = rows.Count(x => x.Status == "Aguardando fila");
                var inProgress = rows.Count(x => x.Status == "Em producao");
                var late = rows.Count(x => x.Status != "Concluida" && x.DueAtUtc < utcNow);
                var efficiency = Math.Clamp(92 - (late * 7) - (pending * 3), 55, 98);

                return new SectorQueueDto(
                    sector,
                    pending,
                    inProgress,
                    late,
                    ResolveSectorOwner(sector),
                    efficiency);
            })
            .ToList();

        var orders = stateStore.ProductionOrders
            .Where(x => x.VisibleInQueues)
            .OrderBy(x => x.DueAtUtc)
            .ThenBy(x => x.Number)
            .Select(MapOrder)
            .ToList();

        return new ProductionOverviewDto(sectors, orders);
    }

    private static ProductionSectorDetailDto MapSector(AppStateStore stateStore, DateTime utcNow, string sector)
    {
        var normalizedSector = SectorFlow.FirstOrDefault(x => x.Equals(sector, StringComparison.OrdinalIgnoreCase)) ?? sector;
        var orders = stateStore.ProductionOrders
            .Where(x => x.VisibleInQueues && x.Sector.Equals(normalizedSector, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.DueAtUtc)
            .ThenBy(x => x.Number)
            .Select(MapOrder)
            .ToList();

        return new ProductionSectorDetailDto(
            normalizedSector,
            ResolveSectorOwner(normalizedSector),
            orders.Count(x => x.Status == "Aguardando fila"),
            orders.Count(x => x.Status == "Em producao"),
            orders.Count(x => x.Status != "Concluida" && x.DueAtUtc < utcNow),
            orders);
    }

    private static ProductionOrderCardDto MapOrder(ProductionOrderState x) => new(
        x.Id,
        x.OrderId,
        x.OrderNumber,
        x.Number,
        x.VisibleInQueues,
        x.ParentProductionOrderId,
        x.ParentProductionOrderNumber,
        x.MergedIntoProductionOrderId,
        x.MergedIntoProductionOrderNumber,
        x.RelatedProductionOrderNumbers,
        x.TraceabilityReason,
        x.CustomerName,
        x.Title,
        x.Quantity,
        x.ProductTemplateId,
        x.ProductName,
        x.BillingMethod,
        x.UnitPrice,
        x.Sector,
        x.Status,
        x.Priority,
        x.Owner,
        x.Complexity,
        x.Outsourced,
        x.MaterialSupport,
        x.DueAtUtc,
        x.UpdatedAtUtc);

    private static ProductionOrderState CloneOrder(
        ProductionOrderState source,
        Guid id,
        string number,
        string title,
        int quantity,
        string sector,
        string owner,
        string priority,
        DateTime dueAtUtc,
        Guid? parentProductionOrderId,
        string? parentProductionOrderNumber,
        Guid? mergedIntoProductionOrderId,
        string? mergedIntoProductionOrderNumber,
        List<string> relatedProductionOrderNumbers,
        string? traceabilityReason) => new()
        {
            Id = id,
            OrderId = source.OrderId,
            OrderNumber = source.OrderNumber,
            Number = number,
            VisibleInQueues = true,
            ParentProductionOrderId = parentProductionOrderId,
            ParentProductionOrderNumber = parentProductionOrderNumber,
            MergedIntoProductionOrderId = mergedIntoProductionOrderId,
            MergedIntoProductionOrderNumber = mergedIntoProductionOrderNumber,
            RelatedProductionOrderNumbers = relatedProductionOrderNumbers,
            TraceabilityReason = string.IsNullOrWhiteSpace(traceabilityReason) ? null : traceabilityReason.Trim(),
            CustomerName = source.CustomerName,
            Title = string.IsNullOrWhiteSpace(title) ? source.Title : title.Trim(),
            Quantity = Math.Max(1, quantity),
            ProductTemplateId = source.ProductTemplateId,
            ProductName = source.ProductName,
            BillingMethod = source.BillingMethod,
            UnitPrice = source.UnitPrice,
            Sector = sector,
            Status = "Aguardando fila",
            Priority = priority,
            Owner = owner,
            Complexity = source.Complexity,
            Outsourced = source.Outsourced,
            MaterialSupport = source.MaterialSupport,
            DueAtUtc = dueAtUtc,
            UpdatedAtUtc = DateTime.UtcNow
        };

    private static string? NormalizeSector(string? sector)
    {
        if (string.IsNullOrWhiteSpace(sector))
        {
            return null;
        }

        return SectorFlow.FirstOrDefault(x => x.Equals(sector.Trim(), StringComparison.OrdinalIgnoreCase)) ?? sector.Trim();
    }

    private static string? ResolveNextSector(string currentSector)
    {
        var index = Array.IndexOf(SectorFlow, currentSector);
        if (index < 0 || index == SectorFlow.Length - 1)
        {
            return null;
        }

        return SectorFlow[index + 1];
    }

    private static string ResolveSectorOwner(string sector) => sector switch
    {
        "Preparacao" => "PCP central",
        "Corte" => "Corte e laser",
        "Montagem" => "Montagem",
        "Emborrachamento" => "Setor de borracha",
        "Expedicao" => "Logistica",
        _ => "Operacao"
    };
}
