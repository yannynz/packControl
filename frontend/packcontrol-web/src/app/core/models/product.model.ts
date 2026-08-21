export interface ProductMaterialRequirement {
  materialId: string;
  materialName: string;
  quantityPerUnit: number;
  unit: string;
}

export interface ProductTemplate {
  id: string;
  name: string;
  category: string;
  description: string;
  billingMethod: string;
  defaultUnitPrice: number;
  defaultProductionSector: string;
  fiscalNcm: string;
  fiscalCfop: string;
  fiscalCommercialUnit: string;
  fiscalOriginCode: string;
  fiscalIcmsSituationCode: string;
  fiscalIpiSituationCode: string;
  fiscalPisSituationCode: string;
  fiscalCofinsSituationCode: string;
  fiscalIcmsRate: number;
  fiscalIpiRate: number;
  fiscalPisRate: number;
  fiscalCofinsRate: number;
  active: boolean;
  materialRequirements: ProductMaterialRequirement[];
  updatedAtUtc: string;
}

export interface ProductTemplatePayload {
  name: string;
  category: string;
  description: string;
  billingMethod: string;
  defaultUnitPrice: number;
  defaultProductionSector: string;
  fiscalNcm: string;
  fiscalCfop: string;
  fiscalCommercialUnit: string;
  fiscalOriginCode: string;
  fiscalIcmsSituationCode: string;
  fiscalIpiSituationCode: string;
  fiscalPisSituationCode: string;
  fiscalCofinsSituationCode: string;
  fiscalIcmsRate: number;
  fiscalIpiRate: number;
  fiscalPisRate: number;
  fiscalCofinsRate: number;
  active: boolean;
  materialRequirements: ProductMaterialRequirement[];
}
