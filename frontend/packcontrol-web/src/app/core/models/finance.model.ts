export interface FinanceEntry {
  id: string;
  orderId?: string | null;
  orderNumber?: string | null;
  type: string;
  status: string;
  description: string;
  counterparty: string;
  amount: number;
  entrySource: string;
  paymentMethod: string;
  notes?: string | null;
  boletoStatus?: string | null;
  boletoNumber?: string | null;
  boletoLine?: string | null;
  dueAtUtc: string;
}

export interface FiscalInvoice {
  id: string;
  financeEntryId?: string | null;
  orderId?: string | null;
  orderNumber?: string | null;
  number: string;
  series: string;
  environment: string;
  accessKey: string;
  protocol: string;
  engineName: string;
  certificateType: string;
  certificateMedia: string;
  natureOfOperation: string;
  cfop: string;
  xmlArchivePath?: string | null;
  danfeArchivePath?: string | null;
  customerName: string;
  status: string;
  amount: number;
  issuedAtUtc: string;
  notes?: string | null;
}

export interface FinanceOverview {
  openReceivablesTotal: number;
  openPayablesTotal: number;
  overdueTotal: number;
  entries: FinanceEntry[];
  invoices: FiscalInvoice[];
}
