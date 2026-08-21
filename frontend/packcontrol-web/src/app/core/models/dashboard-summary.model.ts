export interface DashboardSummary {
  customers: number;
  orders: number;
  draftOrders: number;
  attachmentsAwaitingAnalysis: number;
  approvedOrders: number;
  productionOrdersInProgress: number;
  criticalStockItems: number;
  pendingShipments: number;
  overdueFinanceEntries: number;
}
