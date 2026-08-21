namespace PackControl.Application.Finance;

public interface IFinanceService
{
    Task<FinanceOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    Task<FinanceOverviewDto> CreateEntryAsync(CreateFinanceEntryRequest request, CancellationToken cancellationToken);
    Task<FinanceOverviewDto> SettleAsync(Guid entryId, CancellationToken cancellationToken);
    Task<FinanceOverviewDto> GenerateBoletoAsync(Guid entryId, CancellationToken cancellationToken);
    Task<FinanceOverviewDto> IssueInvoiceAsync(IssueFiscalInvoiceRequest request, CancellationToken cancellationToken);
}
