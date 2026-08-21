namespace PackControl.Application.Dashboard;

public sealed record DashboardSummaryDto(
    int Customers,
    int Orders,
    int DraftOrders,
    int AttachmentsAwaitingAnalysis,
    int ApprovedOrders,
    int ProductionOrdersInProgress,
    int CriticalStockItems,
    int PendingShipments,
    int OverdueFinanceEntries);
