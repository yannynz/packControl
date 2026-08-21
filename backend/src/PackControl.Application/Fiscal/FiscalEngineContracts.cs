namespace PackControl.Application.Fiscal;

public sealed record FiscalCertificateProfile(
    string CertificateType,
    string CertificateMedia,
    string? CertificateLabel,
    string? CertificateSerialNumber);

public sealed record FiscalPartyAddress(
    string PostalCode,
    string Street,
    string StreetNumber,
    string District,
    string City,
    string StateCode,
    string? CityIbgeCode,
    string Country,
    string? Complement,
    string? ReferencePoint);

public sealed record FiscalEmitterProfile(
    Guid CompanyId,
    string TradeName,
    string DocumentNumber,
    string StateRegistration,
    string TaxRegime,
    string FiscalSeries,
    string Environment,
    string AdapterName,
    FiscalCertificateProfile Certificate,
    FiscalPartyAddress Address,
    int NfeNumber);

public sealed record FiscalRecipientProfile(
    Guid? CustomerId,
    string Name,
    string? DocumentNumber,
    string? StateRegistration,
    string TaxpayerIndicator,
    string? Email,
    string? Phone,
    FiscalPartyAddress Address);

public sealed record FiscalNfeItem(
    int LineNumber,
    Guid? ProductTemplateId,
    string Description,
    string CommercialUnit,
    decimal Quantity,
    decimal TaxQuantity,
    decimal UnitPrice,
    decimal GrossAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? BillingMethod,
    string Cfop,
    string Ncm,
    string OriginCode,
    string IcmsSituationCode,
    string IpiSituationCode,
    string PisSituationCode,
    string CofinsSituationCode,
    decimal IcmsRate,
    decimal IcmsBaseAmount,
    decimal IcmsAmount,
    decimal IpiRate,
    decimal IpiAmount,
    decimal PisRate,
    decimal PisAmount,
    decimal CofinsRate,
    decimal CofinsAmount,
    string? Notes);

public sealed record FiscalNfeTotals(
    decimal ProductsAmount,
    decimal DiscountAmount,
    decimal FreightAmount,
    decimal InsuranceAmount,
    decimal OtherAmount,
    decimal IcmsBaseAmount,
    decimal IcmsAmount,
    decimal IpiAmount,
    decimal PisAmount,
    decimal CofinsAmount,
    decimal InvoiceAmount);

public sealed record FiscalNfePayment(
    string PaymentMethod,
    string BillingType,
    string? EntrySource,
    decimal BillingAmount,
    DateTime? DueAtUtc,
    string? BoletoNumber,
    string? BoletoLine);

public sealed record FiscalNfeTransport(
    Guid? ShipmentId,
    Guid? CarrierId,
    string? CarrierName,
    string Mode,
    string FreightMode,
    string? RecipientName,
    string? DriverName,
    string? VehiclePlate,
    DateTime? ScheduledAtUtc);

public sealed record FiscalNfeEmissionRequest(
    Guid FiscalDocumentId,
    FiscalEmitterProfile Emitter,
    FiscalRecipientProfile Recipient,
    string NatureOfOperation,
    string Cfop,
    IReadOnlyList<FiscalNfeItem> Items,
    FiscalNfeTotals Totals,
    FiscalNfePayment Payment,
    FiscalNfeTransport Transport,
    string? AdditionalInformation);

public sealed record FiscalNfeCancellationRequest(
    Guid FiscalDocumentId,
    string AccessKey,
    string Protocol,
    FiscalEmitterProfile Emitter,
    string Justification);

public sealed record FiscalNfeCorrectionLetterRequest(
    Guid FiscalDocumentId,
    string AccessKey,
    string Protocol,
    FiscalEmitterProfile Emitter,
    FiscalRecipientProfile Recipient,
    int SequenceNumber,
    string CorrectionText);

public sealed record FiscalNfeInutilizationRequest(
    Guid CompanyProfileId,
    FiscalEmitterProfile Emitter,
    string Series,
    int StartNumber,
    int EndNumber,
    string Justification);

public sealed record FiscalNfeEmissionResult(
    string Status,
    string AccessKey,
    string Protocol,
    string EngineName,
    string XmlContent,
    string DanfeHtmlContent);

public sealed record FiscalNfeEventResult(
    string Status,
    string Protocol,
    string EngineName,
    string XmlContent,
    string? DisplayHtmlContent);

public sealed record FiscalNfeStatusRequest(
    string AdapterName,
    string Environment,
    string StateCode,
    bool RequireCertificate);

public sealed record FiscalNfeStatusResult(
    string AdapterName,
    string ProviderName,
    bool IsReachable,
    bool IsServiceOperational,
    bool SupportsRealEmission,
    int? StatusCode,
    string Status,
    string Message,
    string? ApplicationVersion,
    string? RawResponse);

public interface IFiscalNfeEngine
{
    Task<FiscalNfeEmissionResult> IssueAsync(FiscalNfeEmissionRequest request, CancellationToken cancellationToken);
    Task<FiscalNfeEventResult> CancelAsync(FiscalNfeCancellationRequest request, CancellationToken cancellationToken);
    Task<FiscalNfeEventResult> CorrectAsync(FiscalNfeCorrectionLetterRequest request, CancellationToken cancellationToken);
    Task<FiscalNfeEventResult> InutilizeAsync(FiscalNfeInutilizationRequest request, CancellationToken cancellationToken);
    Task<FiscalNfeStatusResult> CheckStatusAsync(FiscalNfeStatusRequest request, CancellationToken cancellationToken);
}
