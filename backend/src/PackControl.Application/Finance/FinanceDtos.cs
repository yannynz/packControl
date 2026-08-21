namespace PackControl.Application.Finance;

public sealed record FinanceEntryDto(
    Guid Id,
    Guid? OrderId,
    string? OrderNumber,
    string Type,
    string Status,
    string Description,
    string Counterparty,
    decimal Amount,
    string EntrySource,
    string PaymentMethod,
    string? Notes,
    string? BoletoStatus,
    string? BoletoNumber,
    string? BoletoLine,
    DateTime DueAtUtc);

public sealed record FiscalInvoiceDto(
    Guid Id,
    Guid? FinanceEntryId,
    Guid? OrderId,
    string? OrderNumber,
    string Number,
    string Series,
    string Environment,
    string AccessKey,
    string Protocol,
    string EngineName,
    string CertificateType,
    string CertificateMedia,
    string NatureOfOperation,
    string Cfop,
    string? XmlArchivePath,
    string? DanfeArchivePath,
    string CustomerName,
    string Status,
    decimal Amount,
    DateTime IssuedAtUtc,
    string? Notes);

public sealed record FinanceOverviewDto(
    decimal OpenReceivablesTotal,
    decimal OpenPayablesTotal,
    decimal OverdueTotal,
    IReadOnlyList<FinanceEntryDto> Entries,
    IReadOnlyList<FiscalInvoiceDto> Invoices);
