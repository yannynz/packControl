export interface MaterialCard {
  id: string;
  name: string;
  technicalType: string;
  category: string;
  mainSupplier: string;
  riskLevel: string;
  standardCost: number;
  leadTimeDays: number;
  unit: string;
}

export interface StockItem {
  id: string;
  materialId: string;
  materialName: string;
  onHand: number;
  reserved: number;
  available: number;
  reorderPoint: number;
  status: string;
  lastMovement: string;
  lastMovementAtUtc: string;
}
