namespace PackControl.Application.Products;

public sealed record ProductMaterialRequirementDto(
    Guid MaterialId,
    string MaterialName,
    decimal QuantityPerUnit,
    string Unit);

public sealed record ProductTemplateDto(
    Guid Id,
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
    IReadOnlyList<ProductMaterialRequirementDto> MaterialRequirements,
    DateTime UpdatedAtUtc);
