using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Products;
using PackControl.Domain.Audit;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class ProductService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IAppStatePersistence statePersistence) : IProductService
{
    public async Task<IReadOnlyList<ProductTemplateDto>> ListAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return stateStore.ProductTemplates
                .OrderBy(x => x.Name)
                .Select(Map)
                .ToList();
        }
    }

    public async Task<ProductTemplateDto> CreateAsync(CreateProductTemplateRequest request, CancellationToken cancellationToken)
    {
        ProductTemplateDto templateDto;
        lock (stateStore.SyncRoot)
        {
            var template = new ProductTemplateState
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Category = request.Category.Trim(),
                Description = request.Description.Trim(),
                BillingMethod = request.BillingMethod.Trim(),
                DefaultUnitPrice = decimal.Round(Math.Max(0, request.DefaultUnitPrice), 2),
                DefaultProductionSector = request.DefaultProductionSector.Trim(),
                FiscalNcm = request.FiscalNcm.Trim(),
                FiscalCfop = request.FiscalCfop.Trim(),
                FiscalCommercialUnit = request.FiscalCommercialUnit.Trim().ToUpperInvariant(),
                FiscalOriginCode = request.FiscalOriginCode.Trim(),
                FiscalIcmsSituationCode = request.FiscalIcmsSituationCode.Trim().ToUpperInvariant(),
                FiscalIpiSituationCode = request.FiscalIpiSituationCode.Trim().ToUpperInvariant(),
                FiscalPisSituationCode = request.FiscalPisSituationCode.Trim().ToUpperInvariant(),
                FiscalCofinsSituationCode = request.FiscalCofinsSituationCode.Trim().ToUpperInvariant(),
                FiscalIcmsRate = decimal.Round(Math.Max(0, request.FiscalIcmsRate), 2),
                FiscalIpiRate = decimal.Round(Math.Max(0, request.FiscalIpiRate), 2),
                FiscalPisRate = decimal.Round(Math.Max(0, request.FiscalPisRate), 4),
                FiscalCofinsRate = decimal.Round(Math.Max(0, request.FiscalCofinsRate), 4),
                Active = request.Active,
                MaterialRequirements = MapRequirements(request.MaterialRequirements),
                UpdatedAtUtc = clock.UtcNow
            };

            stateStore.ProductTemplates.Add(template);
            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(ProductTemplateState),
                template.Id,
                "product_template.created",
                $"Produto comercial {template.Name} criado.",
                JsonSerializer.Serialize(new { template.BillingMethod, template.DefaultUnitPrice, template.FiscalNcm, template.FiscalCfop }),
                clock.UtcNow));

            templateDto = Map(template);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return templateDto;
    }

    public async Task<ProductTemplateDto?> UpdateAsync(
        Guid productTemplateId,
        UpdateProductTemplateRequest request,
        CancellationToken cancellationToken)
    {
        ProductTemplateDto? templateDto;
        lock (stateStore.SyncRoot)
        {
            var template = stateStore.ProductTemplates.SingleOrDefault(x => x.Id == productTemplateId);
            if (template is null)
            {
                return null;
            }

            template.Name = request.Name.Trim();
            template.Category = request.Category.Trim();
            template.Description = request.Description.Trim();
            template.BillingMethod = request.BillingMethod.Trim();
            template.DefaultUnitPrice = decimal.Round(Math.Max(0, request.DefaultUnitPrice), 2);
            template.DefaultProductionSector = request.DefaultProductionSector.Trim();
            template.FiscalNcm = request.FiscalNcm.Trim();
            template.FiscalCfop = request.FiscalCfop.Trim();
            template.FiscalCommercialUnit = request.FiscalCommercialUnit.Trim().ToUpperInvariant();
            template.FiscalOriginCode = request.FiscalOriginCode.Trim();
            template.FiscalIcmsSituationCode = request.FiscalIcmsSituationCode.Trim().ToUpperInvariant();
            template.FiscalIpiSituationCode = request.FiscalIpiSituationCode.Trim().ToUpperInvariant();
            template.FiscalPisSituationCode = request.FiscalPisSituationCode.Trim().ToUpperInvariant();
            template.FiscalCofinsSituationCode = request.FiscalCofinsSituationCode.Trim().ToUpperInvariant();
            template.FiscalIcmsRate = decimal.Round(Math.Max(0, request.FiscalIcmsRate), 2);
            template.FiscalIpiRate = decimal.Round(Math.Max(0, request.FiscalIpiRate), 2);
            template.FiscalPisRate = decimal.Round(Math.Max(0, request.FiscalPisRate), 4);
            template.FiscalCofinsRate = decimal.Round(Math.Max(0, request.FiscalCofinsRate), 4);
            template.Active = request.Active;
            template.MaterialRequirements = MapRequirements(request.MaterialRequirements);
            template.UpdatedAtUtc = clock.UtcNow;

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(ProductTemplateState),
                template.Id,
                "product_template.updated",
                $"Produto comercial {template.Name} atualizado.",
                JsonSerializer.Serialize(new { template.BillingMethod, template.DefaultUnitPrice, template.FiscalNcm, template.FiscalCfop }),
                clock.UtcNow));

            templateDto = Map(template);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return templateDto;
    }

    private static List<ProductMaterialRequirementState> MapRequirements(
        IReadOnlyList<ProductMaterialRequirementRequest> requirements) =>
        requirements
            .Where(x => x.MaterialId != Guid.Empty && x.QuantityPerUnit > 0)
            .Select(x => new ProductMaterialRequirementState
            {
                MaterialId = x.MaterialId,
                MaterialName = x.MaterialName.Trim(),
                QuantityPerUnit = x.QuantityPerUnit,
                Unit = x.Unit.Trim()
            })
            .ToList();

    private static ProductTemplateDto Map(ProductTemplateState template) => new(
        template.Id,
        template.Name,
        template.Category,
        template.Description,
        template.BillingMethod,
        template.DefaultUnitPrice,
        template.DefaultProductionSector,
        template.FiscalNcm,
        template.FiscalCfop,
        template.FiscalCommercialUnit,
        template.FiscalOriginCode,
        template.FiscalIcmsSituationCode,
        template.FiscalIpiSituationCode,
        template.FiscalPisSituationCode,
        template.FiscalCofinsSituationCode,
        template.FiscalIcmsRate,
        template.FiscalIpiRate,
        template.FiscalPisRate,
        template.FiscalCofinsRate,
        template.Active,
        template.MaterialRequirements
            .Select(x => new ProductMaterialRequirementDto(x.MaterialId, x.MaterialName, x.QuantityPerUnit, x.Unit))
            .ToList(),
        template.UpdatedAtUtc);
}
