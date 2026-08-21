export interface SectorQueue {
  name: string;
  pending: number;
  inProgress: number;
  late: number;
  defaultOwner: string;
  efficiencyPercent: number;
}

export interface ProductionOrderCard {
  id: string;
  orderId: string;
  orderNumber: string;
  number: string;
  visibleInQueues: boolean;
  parentProductionOrderId?: string | null;
  parentProductionOrderNumber?: string | null;
  mergedIntoProductionOrderId?: string | null;
  mergedIntoProductionOrderNumber?: string | null;
  relatedProductionOrderNumbers: string[];
  traceabilityReason?: string | null;
  customerName: string;
  title: string;
  quantity: number;
  productTemplateId?: string | null;
  productName?: string | null;
  billingMethod: string;
  unitPrice?: number | null;
  sector: string;
  status: string;
  priority: string;
  owner: string;
  complexity: number;
  outsourced: boolean;
  materialSupport: string;
  dueAtUtc: string;
  updatedAtUtc: string;
}

export interface ProductionOverview {
  sectors: SectorQueue[];
  orders: ProductionOrderCard[];
}

export interface ProductionSectorDetail {
  name: string;
  defaultOwner: string;
  pending: number;
  inProgress: number;
  late: number;
  orders: ProductionOrderCard[];
}
