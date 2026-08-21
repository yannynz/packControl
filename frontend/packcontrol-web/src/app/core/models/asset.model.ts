export interface TechnicalAsset {
  id: string;
  customerId: string;
  customerName: string;
  code: string;
  alias: string;
  assetType: string;
  status: string;
  revision: string;
  components: string[];
  materials: string[];
  lastOrderNumber?: string | null;
  notes: string;
  updatedAtUtc: string;
}

export interface TechnicalAssetPayload {
  customerId: string;
  code: string;
  alias: string;
  assetType: string;
  status: string;
  revision: string;
  components: string[];
  materials: string[];
  lastOrderNumber?: string | null;
  notes: string;
}
