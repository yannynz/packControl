namespace PackControl.Application.Finance;

public sealed record IssueFiscalInvoiceRequest(
    Guid? FinanceEntryId,
    string Series,
    string NatureOfOperation,
    string Cfop,
    string? Notes);
