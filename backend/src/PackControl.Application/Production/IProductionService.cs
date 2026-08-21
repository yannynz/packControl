namespace PackControl.Application.Production;

public interface IProductionService
{
    Task<ProductionOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    Task<ProductionSectorDetailDto> GetSectorAsync(string sector, CancellationToken cancellationToken);
    Task<ProductionOverviewDto> AdvanceAsync(Guid productionOrderId, CancellationToken cancellationToken);
    Task<ProductionOverviewDto> SplitAsync(Guid productionOrderId, SplitProductionOrderRequest request, CancellationToken cancellationToken);
    Task<ProductionOverviewDto> MergeAsync(MergeProductionOrdersRequest request, CancellationToken cancellationToken);
}
