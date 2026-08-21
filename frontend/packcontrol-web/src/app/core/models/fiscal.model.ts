export interface FiscalDocument {
  id: string;
  companyProfileId: string;
  financeEntryId?: string | null;
  orderId?: string | null;
  orderNumber?: string | null;
  number: string;
  series: string;
  environment: string;
  accessKey: string;
  protocol?: string | null;
  adapterName: string;
  issueMode: string;
  certificateType: string;
  certificateMedia: string;
  natureOfOperation: string;
  cfop: string;
  recipientName: string;
  recipientDocument?: string | null;
  amount: number;
  status: string;
  lastError?: string | null;
  attemptsCount: number;
  xmlArchivePath?: string | null;
  danfeArchivePath?: string | null;
  createdAtUtc: string;
  issuedAtUtc?: string | null;
  updatedAtUtc: string;
  emitter: FiscalDocumentEmitter;
  recipient: FiscalDocumentRecipient;
  items: FiscalDocumentItem[];
  totals: FiscalDocumentTotals;
  payment: FiscalDocumentPayment;
  transport: FiscalDocumentTransport;
  events: FiscalDocumentEvent[];
  artifacts: FiscalDocumentArtifact[];
  notes?: string | null;
}

export interface FiscalDocumentEvent {
  id: string;
  eventType: string;
  description: string;
  payloadJson?: string | null;
  actorUserId?: string | null;
  actorName: string;
  occurredAtUtc: string;
}

export interface FiscalDocumentArtifact {
  id: string;
  kind: string;
  fileName: string;
  storagePath: string;
  contentType: string;
  sizeBytes: number;
  sha256: string;
  createdAtUtc: string;
}

export interface FiscalAddress {
  postalCode: string;
  street: string;
  streetNumber: string;
  district: string;
  city: string;
  stateCode: string;
  cityIbgeCode?: string | null;
  country: string;
  complement?: string | null;
  referencePoint?: string | null;
}

export interface FiscalDocumentEmitter {
  companyId: string;
  tradeName: string;
  documentNumber: string;
  stateRegistration: string;
  taxRegime: string;
  fiscalSeries: string;
  environment: string;
  address: FiscalAddress;
}

export interface FiscalDocumentRecipient {
  customerId?: string | null;
  name: string;
  documentNumber?: string | null;
  stateRegistration?: string | null;
  taxpayerIndicator: string;
  email?: string | null;
  phone?: string | null;
  address: FiscalAddress;
}

export interface FiscalDocumentItem {
  lineNumber: number;
  productTemplateId?: string | null;
  description: string;
  commercialUnit: string;
  quantity: number;
  taxQuantity: number;
  unitPrice: number;
  grossAmount: number;
  discountAmount: number;
  totalAmount: number;
  billingMethod?: string | null;
  cfop: string;
  ncm: string;
  originCode: string;
  icmsSituationCode: string;
  ipiSituationCode: string;
  pisSituationCode: string;
  cofinsSituationCode: string;
  icmsRate: number;
  icmsBaseAmount: number;
  icmsAmount: number;
  ipiRate: number;
  ipiAmount: number;
  pisRate: number;
  pisAmount: number;
  cofinsRate: number;
  cofinsAmount: number;
  notes?: string | null;
}

export interface FiscalDocumentTotals {
  productsAmount: number;
  discountAmount: number;
  freightAmount: number;
  insuranceAmount: number;
  otherAmount: number;
  icmsBaseAmount: number;
  icmsAmount: number;
  ipiAmount: number;
  pisAmount: number;
  cofinsAmount: number;
  invoiceAmount: number;
}

export interface FiscalDocumentPayment {
  paymentMethod: string;
  billingType: string;
  entrySource?: string | null;
  billingAmount: number;
  dueAtUtc?: string | null;
  boletoNumber?: string | null;
  boletoLine?: string | null;
}

export interface FiscalDocumentTransport {
  shipmentId?: string | null;
  carrierId?: string | null;
  carrierName?: string | null;
  mode: string;
  freightMode: string;
  recipientName?: string | null;
  driverName?: string | null;
  vehiclePlate?: string | null;
  scheduledAtUtc?: string | null;
}

export interface FiscalCompanyProfileItem {
  id: string;
  tradeName: string;
  documentNumber: string;
  stateRegistration: string;
  taxRegime: string;
  postalCode: string;
  street: string;
  streetNumber: string;
  district: string;
  city: string;
  stateCode: string;
  cityIbgeCode: string;
  country: string;
  complement?: string | null;
  fiscalSeries: string;
  nfeEnabled: boolean;
  environment: string;
  adapterName: string;
  certificateType: string;
  certificateMedia: string;
  principalEmissionMode: string;
  contingencyEmissionMode?: string | null;
  certificateLabel?: string | null;
  certificateSerialNumber?: string | null;
  accountantValidated: boolean;
  homologationCredentialsValidated: boolean;
  homologationApproved: boolean;
  productionCredentialsValidated: boolean;
  productionApproved: boolean;
  onboardingStatus: string;
  canStartHomologation: boolean;
  canIssueInCurrentEnvironment: boolean;
  canGoLive: boolean;
  blockingIssues: string[];
  pendingActions: string[];
  onboardingNotes?: string | null;
}

export interface FiscalOperationTemplate {
  id: string;
  companyProfileId?: string | null;
  name: string;
  natureOfOperation: string;
  cfop: string;
  finality: string;
  active: boolean;
  notes?: string | null;
  updatedAtUtc: string;
}

export interface FiscalAgentRegistration {
  id: string;
  name: string;
  hostname: string;
  certificateMedia: string;
  online: boolean;
  lastSeenAtUtc: string;
  status: string;
  notes?: string | null;
}

export interface FiscalEngineDiagnostic {
  companyProfileId: string;
  adapterName: string;
  providerName: string;
  environment: string;
  stateCode: string;
  isReachable: boolean;
  isServiceOperational: boolean;
  supportsRealEmission: boolean;
  canIssueRealNfe: boolean;
  statusCode?: number | null;
  status: string;
  message: string;
  applicationVersion?: string | null;
  blockingIssues: string[];
  rawResponse?: string | null;
  checkedAtUtc: string;
}

export interface FiscalNumberingEvent {
  id: string;
  companyProfileId: string;
  series: string;
  startNumber: number;
  endNumber: number;
  environment: string;
  adapterName: string;
  protocol: string;
  status: string;
  reason: string;
  xmlArchivePath?: string | null;
  previewArchivePath?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface FiscalOverview {
  companies: FiscalCompanyProfileItem[];
  operationTemplates: FiscalOperationTemplate[];
  agents: FiscalAgentRegistration[];
  documents: FiscalDocument[];
  numberingEvents: FiscalNumberingEvent[];
}
