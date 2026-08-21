using PackControl.Application.Dashboard;
using PackControl.Domain.Orders;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class DashboardService(AppStateStore stateStore) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            var customers = stateStore.Customers.Count;
            var orders = stateStore.Orders.Count;
            var draftOrders = stateStore.Orders.Count(x => x.Status == OrderStatus.Draft);
            var approvedOrders = stateStore.Orders.Count(x => x.Status == OrderStatus.Approved);
            var attachmentsAwaitingAnalysis = stateStore.Orders
                .SelectMany(x => x.Analyses)
                .Count(x => x.Status == TechnicalAnalysisStatus.PendingEngine);
            var productionOrdersInProgress = stateStore.ProductionOrders.Count(x => x.Status == "Em producao");
            var criticalStockItems = stateStore.StockItems.Count(x => (x.OnHand - x.Reserved) <= x.ReorderPoint * 0.5m);
            var pendingShipments = stateStore.Shipments.Count(x => x.Status is "Aguardando producao" or "Pronto para expedir" or "Retirada agendada");
            var overdueFinanceEntries = stateStore.FinanceEntries.Count(x => x.Status != "Liquidado" && x.DueAtUtc.Date < DateTime.UtcNow.Date);

            return new DashboardSummaryDto(
                customers,
                orders,
                draftOrders,
                attachmentsAwaitingAnalysis,
                approvedOrders,
                productionOrdersInProgress,
                criticalStockItems,
                pendingShipments,
                overdueFinanceEntries);
        }
    }
}
