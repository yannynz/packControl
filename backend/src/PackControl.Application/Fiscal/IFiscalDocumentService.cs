namespace PackControl.Application.Fiscal;

public interface IFiscalDocumentService
{
    Task<FiscalOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    Task<FiscalEngineDiagnosticDto> GetEngineDiagnosticAsync(Guid? companyProfileId, CancellationToken cancellationToken);
    Task<FiscalDocumentDto> PrepareAsync(PrepareFiscalDocumentCommand command, CancellationToken cancellationToken);
    Task<FiscalDocumentDto> IssueAsync(IssueFiscalDocumentCommand command, CancellationToken cancellationToken);
    Task<FiscalDocumentDto> CancelAsync(CancelFiscalDocumentCommand command, CancellationToken cancellationToken);
    Task<FiscalDocumentDto> ApplyCorrectionLetterAsync(ApplyFiscalCorrectionLetterCommand command, CancellationToken cancellationToken);
    Task<FiscalOverviewDto> InutilizeNumberRangeAsync(InutilizeFiscalNumberRangeCommand command, CancellationToken cancellationToken);
    Task<FiscalOverviewDto> UpdateCompanyProfileAsync(Guid companyProfileId, UpdateFiscalCompanyProfileCommand command, CancellationToken cancellationToken);
    Task<FiscalOverviewDto> UpsertOperationTemplateAsync(Guid? templateId, UpsertFiscalOperationTemplateCommand command, CancellationToken cancellationToken);
}
