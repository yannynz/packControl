namespace PackControl.Application.Logistics;

public interface ILogisticsService
{
    Task<LogisticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    Task<LogisticsOverviewDto> DispatchAsync(Guid shipmentId, CancellationToken cancellationToken);
    Task<LogisticsOverviewDto> MarkWithdrawalAsync(Guid shipmentId, CancellationToken cancellationToken);
    Task<LogisticsOverviewDto> MarkAdverseAsync(Guid shipmentId, CancellationToken cancellationToken);
}
