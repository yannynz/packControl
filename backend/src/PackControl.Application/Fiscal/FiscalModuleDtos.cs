namespace PackControl.Application.Fiscal;

public sealed record PrepareFiscalDocumentCommand(
    Guid? FinanceEntryId,
    Guid? OrderId,
    string? Series,
    string? NatureOfOperation,
    string? Cfop,
    string? Notes);

public sealed record IssueFiscalDocumentCommand(
    Guid? FiscalDocumentId,
    Guid? FinanceEntryId,
    Guid? OrderId,
    string? Series,
    string? NatureOfOperation,
    string? Cfop,
    string? Notes);

public sealed record CancelFiscalDocumentCommand(
    Guid FiscalDocumentId,
    string Reason);

public sealed record ApplyFiscalCorrectionLetterCommand(
    Guid FiscalDocumentId,
    string CorrectionText);

public sealed record InutilizeFiscalNumberRangeCommand(
    Guid CompanyProfileId,
    string Series,
    int StartNumber,
    int EndNumber,
    string Reason);

public sealed record UpsertFiscalOperationTemplateCommand(
    Guid? CompanyProfileId,
    string Name,
    string NatureOfOperation,
    string Cfop,
    string Finality,
    bool Active,
    string? Notes);

public sealed record UpdateFiscalCompanyProfileCommand(
    string TradeName,
    string DocumentNumber,
    string StateRegistration,
    string TaxRegime,
    string PostalCode,
    string Street,
    string StreetNumber,
    string District,
    string City,
    string StateCode,
    string CityIbgeCode,
    string Country,
    string? Complement,
    string FiscalSeries,
    bool NfeEnabled,
    string Environment,
    string AdapterName,
    string CertificateType,
    string CertificateMedia,
    string PrincipalEmissionMode,
    string? ContingencyEmissionMode,
    string? CertificateLabel,
    string? CertificateSerialNumber,
    bool AccountantValidated,
    bool HomologationCredentialsValidated,
    bool HomologationApproved,
    bool ProductionCredentialsValidated,
    bool ProductionApproved,
    string? OnboardingNotes);

public sealed record FiscalDocumentDto(
    Guid Id,
    Guid CompanyProfileId,
    Guid? FinanceEntryId,
    Guid? OrderId,
    string? OrderNumber,
    string Number,
    string Series,
    string Environment,
    string AccessKey,
    string? Protocol,
    string AdapterName,
    string IssueMode,
    string CertificateType,
    string CertificateMedia,
    string NatureOfOperation,
    string Cfop,
    string RecipientName,
    string? RecipientDocument,
    decimal Amount,
    string Status,
    string? LastError,
    int AttemptsCount,
    string? XmlArchivePath,
    string? DanfeArchivePath,
    DateTime CreatedAtUtc,
    DateTime? IssuedAtUtc,
    DateTime UpdatedAtUtc,
    FiscalDocumentEmitterDto Emitter,
    FiscalDocumentRecipientDto Recipient,
    IReadOnlyList<FiscalDocumentItemDto> Items,
    FiscalDocumentTotalsDto Totals,
    FiscalDocumentPaymentDto Payment,
    FiscalDocumentTransportDto Transport,
    IReadOnlyList<FiscalDocumentEventDto> Events,
    IReadOnlyList<FiscalDocumentArtifactDto> Artifacts,
    string? Notes);

public sealed record FiscalDocumentEventDto(
    Guid Id,
    string EventType,
    string Description,
    string? PayloadJson,
    Guid? ActorUserId,
    string ActorName,
    DateTime OccurredAtUtc);

public sealed record FiscalDocumentArtifactDto(
    Guid Id,
    string Kind,
    string FileName,
    string StoragePath,
    string ContentType,
    long SizeBytes,
    string Sha256,
    DateTime CreatedAtUtc);

public sealed record FiscalAddressDto(
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

public sealed record FiscalDocumentEmitterDto(
    Guid CompanyId,
    string TradeName,
    string DocumentNumber,
    string StateRegistration,
    string TaxRegime,
    string FiscalSeries,
    string Environment,
    FiscalAddressDto Address);

public sealed record FiscalDocumentRecipientDto(
    Guid? CustomerId,
    string Name,
    string? DocumentNumber,
    string? StateRegistration,
    string TaxpayerIndicator,
    string? Email,
    string? Phone,
    FiscalAddressDto Address);

public sealed record FiscalDocumentItemDto(
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

public sealed record FiscalDocumentTotalsDto(
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

public sealed record FiscalDocumentPaymentDto(
    string PaymentMethod,
    string BillingType,
    string? EntrySource,
    decimal BillingAmount,
    DateTime? DueAtUtc,
    string? BoletoNumber,
    string? BoletoLine);

public sealed record FiscalDocumentTransportDto(
    Guid? ShipmentId,
    Guid? CarrierId,
    string? CarrierName,
    string Mode,
    string FreightMode,
    string? RecipientName,
    string? DriverName,
    string? VehiclePlate,
    DateTime? ScheduledAtUtc);

public sealed record FiscalCompanyProfileItemDto(
    Guid Id,
    string TradeName,
    string DocumentNumber,
    string StateRegistration,
    string TaxRegime,
    string PostalCode,
    string Street,
    string StreetNumber,
    string District,
    string City,
    string StateCode,
    string CityIbgeCode,
    string Country,
    string? Complement,
    string FiscalSeries,
    bool NfeEnabled,
    string Environment,
    string AdapterName,
    string CertificateType,
    string CertificateMedia,
    string PrincipalEmissionMode,
    string? ContingencyEmissionMode,
    string? CertificateLabel,
    string? CertificateSerialNumber,
    bool AccountantValidated,
    bool HomologationCredentialsValidated,
    bool HomologationApproved,
    bool ProductionCredentialsValidated,
    bool ProductionApproved,
    string OnboardingStatus,
    bool CanStartHomologation,
    bool CanIssueInCurrentEnvironment,
    bool CanGoLive,
    IReadOnlyList<string> BlockingIssues,
    IReadOnlyList<string> PendingActions,
    string? OnboardingNotes);

public sealed record FiscalOperationTemplateDto(
    Guid Id,
    Guid? CompanyProfileId,
    string Name,
    string NatureOfOperation,
    string Cfop,
    string Finality,
    bool Active,
    string? Notes,
    DateTime UpdatedAtUtc);

public sealed record FiscalAgentRegistrationDto(
    Guid Id,
    string Name,
    string Hostname,
    string CertificateMedia,
    bool Online,
    DateTime LastSeenAtUtc,
    string Status,
    string? Notes);

public sealed record FiscalEngineDiagnosticDto(
    Guid CompanyProfileId,
    string AdapterName,
    string ProviderName,
    string Environment,
    string StateCode,
    bool IsReachable,
    bool IsServiceOperational,
    bool SupportsRealEmission,
    bool CanIssueRealNfe,
    int? StatusCode,
    string Status,
    string Message,
    string? ApplicationVersion,
    IReadOnlyList<string> BlockingIssues,
    string? RawResponse,
    DateTime CheckedAtUtc);

public sealed record FiscalNumberingEventDto(
    Guid Id,
    Guid CompanyProfileId,
    string Series,
    int StartNumber,
    int EndNumber,
    string Environment,
    string AdapterName,
    string Protocol,
    string Status,
    string Reason,
    string? XmlArchivePath,
    string? PreviewArchivePath,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record FiscalOverviewDto(
    IReadOnlyList<FiscalCompanyProfileItemDto> Companies,
    IReadOnlyList<FiscalOperationTemplateDto> OperationTemplates,
    IReadOnlyList<FiscalAgentRegistrationDto> Agents,
    IReadOnlyList<FiscalDocumentDto> Documents,
    IReadOnlyList<FiscalNumberingEventDto> NumberingEvents);
