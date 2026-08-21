export interface Shipment {
  id: string;
  orderId: string;
  orderNumber: string;
  shipmentNumber: string;
  customerName: string;
  mode: string;
  status: string;
  recipient: string;
  carrierId?: string | null;
  carrierName?: string | null;
  driverName: string;
  vehiclePlate: string;
  checklistStatus: string;
  hasOccurrence: boolean;
  scheduledAtUtc: string;
}

export interface LogisticsOverview {
  pendingShipments: number;
  todayShipments: number;
  adverseShipments: number;
  shipments: Shipment[];
}
