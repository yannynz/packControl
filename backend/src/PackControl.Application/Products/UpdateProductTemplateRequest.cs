namespace PackControl.Application.Products;

public sealed record UpdateProductTemplateRequest(
    string Name,
    string Category,
    string Description,
    string BillingMethod,
    decimal DefaultUnitPrice,
    string DefaultProductionSector,
    string FiscalNcm,
    string FiscalCfop,
    string FiscalCommercialUnit,
    string FiscalOriginCode,
    string FiscalIcmsSituationCode,
    string FiscalIpiSituationCode,
    string FiscalPisSituationCode,
    string FiscalCofinsSituationCode,
    decimal FiscalIcmsRate,
    decimal FiscalIpiRate,
    decimal FiscalPisRate,
    decimal FiscalCofinsRate,
    bool Active,
    IReadOnlyList<ProductMaterialRequirementRequest> MaterialRequirements);
