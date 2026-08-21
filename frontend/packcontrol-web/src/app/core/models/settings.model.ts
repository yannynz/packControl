export interface AccessUser {
  id: string;
  name: string;
  email: string;
  role: string;
  mfaRequired: boolean;
  active: boolean;
}

export interface EstimatorParameter {
  label: string;
  value: string;
  unit: string;
}

export interface CompanyProfile {
  tradeName: string;
  documentNumber: string;
  stateRegistration: string;
  fiscalSeries: string;
  nfeEnabled: boolean;
  environment: string;
  adapterName: string;
  certificateType: string;
  certificateMedia: string;
}

export interface IntegrationStatus {
  name: string;
  status: string;
  notes: string;
}

export interface SettingsOverview {
  users: AccessUser[];
  estimatorParameters: EstimatorParameter[];
  companies: CompanyProfile[];
  integrations: IntegrationStatus[];
}
