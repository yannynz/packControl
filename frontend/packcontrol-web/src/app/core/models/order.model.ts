export interface ScopeItemPayload {
  title: string;
  category: string;
  quantity: number;
  productTemplateId?: string | null;
  productName?: string | null;
  billingMethod?: string | null;
  unitPrice?: number | null;
  notes?: string | null;
}

export interface CreateOrderPayload {
  customerId: string;
  serviceType: 'New' | 'Repeat' | 'Maintenance' | 'Rework' | 'Adaptation';
  urgency: 'Normal' | 'Urgent' | 'MachineStop';
  contextSummary?: string | null;
  legacyAssetReference?: string | null;
  notes?: string | null;
  scopeItems: ScopeItemPayload[];
}

export interface OrderListItem {
  id: string;
  number: string;
  customerName: string;
  status: string;
  serviceType: string;
  urgency: string;
  scopePreview: string;
  scopeItemsCount: number;
  attachmentsCount: number;
  createdAtUtc: string;
}

export interface OrderScopeItem {
  id: string;
  title: string;
  category: string;
  quantity: number;
  productTemplateId?: string | null;
  productName?: string | null;
  billingMethod?: string | null;
  unitPrice?: number | null;
  notes?: string | null;
}

export interface OrderAttachment {
  id: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  sha256: string;
  uploadedAtUtc: string;
}

export interface TechnicalAnalysis {
  id: string;
  attachmentId: string;
  sourceFileExtension: string;
  status: string;
  summary: string;
  engineName?: string | null;
  confidencePercent?: number | null;
  createdAtUtc: string;
}

export interface RelatedProductionOrder {
  id: string;
  number: string;
  sector: string;
  status: string;
  owner: string;
  dueAtUtc: string;
}

export interface RelatedShipment {
  id: string;
  shipmentNumber: string;
  mode: string;
  status: string;
  recipient: string;
  carrierName?: string | null;
  scheduledAtUtc: string;
}

export interface RelatedFinanceEntry {
  id: string;
  type: string;
  status: string;
  description: string;
  amount: number;
  entrySource: string;
  paymentMethod: string;
  boletoStatus?: string | null;
  boletoNumber?: string | null;
  dueAtUtc: string;
}

export interface OrderHistoryEntry {
  id: string;
  eventType: string;
  description: string;
  actor: string;
  occurredAtUtc: string;
}

export interface OrderDetail {
  id: string;
  number: string;
  customerId: string;
  customerName: string;
  status: string;
  serviceType: string;
  urgency: string;
  contextSummary?: string | null;
  legacyAssetReference?: string | null;
  notes?: string | null;
  scopeItems: OrderScopeItem[];
  attachments: OrderAttachment[];
  analyses: TechnicalAnalysis[];
  productionOrders: RelatedProductionOrder[];
  shipments: RelatedShipment[];
  financeEntries: RelatedFinanceEntry[];
  history: OrderHistoryEntry[];
  createdAtUtc: string;
}
