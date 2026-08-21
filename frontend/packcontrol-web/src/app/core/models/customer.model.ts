export interface CustomerProductPricingRule {
  productTemplateId: string;
  productName: string;
  billingMethod: string;
  unitPrice: number;
  notes?: string | null;
}

export interface Customer {
  id: string;
  name: string;
  documentNumber?: string | null;
  contactName?: string | null;
  email?: string | null;
  phone?: string | null;
  notes?: string | null;
  nicknames: string[];
  postalCode?: string | null;
  street?: string | null;
  streetNumber?: string | null;
  district?: string | null;
  city?: string | null;
  state?: string | null;
  cityIbgeCode?: string | null;
  stateRegistration?: string | null;
  taxpayerIndicator: string;
  complement?: string | null;
  referencePoint?: string | null;
  defaultCarrierId?: string | null;
  defaultCarrierName?: string | null;
  defaultDeliveryMode?: string | null;
  productPricingRules: CustomerProductPricingRule[];
  score: number;
}

export interface CustomerPayload {
  name: string;
  documentNumber?: string | null;
  contactName?: string | null;
  email?: string | null;
  phone?: string | null;
  notes?: string | null;
  nicknames: string[];
  postalCode?: string | null;
  street?: string | null;
  streetNumber?: string | null;
  district?: string | null;
  city?: string | null;
  state?: string | null;
  cityIbgeCode?: string | null;
  stateRegistration?: string | null;
  taxpayerIndicator: string;
  complement?: string | null;
  referencePoint?: string | null;
  defaultCarrierId?: string | null;
  defaultCarrierName?: string | null;
  defaultDeliveryMode?: string | null;
  productPricingRules: CustomerProductPricingRule[];
  score: number;
}
