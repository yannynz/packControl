namespace PackControl.Infrastructure.Persistence;

public sealed class ProductionOrderState
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public bool VisibleInQueues { get; set; } = true;
    public Guid? ParentProductionOrderId { get; set; }
    public string? ParentProductionOrderNumber { get; set; }
    public Guid? MergedIntoProductionOrderId { get; set; }
    public string? MergedIntoProductionOrderNumber { get; set; }
    public List<string> RelatedProductionOrderNumbers { get; set; } = [];
    public string? TraceabilityReason { get; set; }
    public string CustomerName { get; init; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid? ProductTemplateId { get; set; }
    public string? ProductName { get; set; }
    public string BillingMethod { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public int Complexity { get; set; }
    public bool Outsourced { get; set; }
    public string MaterialSupport { get; set; } = string.Empty;
    public DateTime DueAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class MaterialCatalogItemState
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TechnicalType { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string MainSupplier { get; init; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public decimal StandardCost { get; set; }
    public int LeadTimeDays { get; set; }
    public string Unit { get; init; } = string.Empty;
}

public sealed class StockItemState
{
    public Guid Id { get; init; }
    public Guid MaterialId { get; init; }
    public string MaterialName { get; init; } = string.Empty;
    public decimal OnHand { get; set; }
    public decimal Reserved { get; set; }
    public decimal ReorderPoint { get; set; }
    public string LastMovement { get; set; } = string.Empty;
    public DateTime LastMovementAtUtc { get; set; }
}

public sealed class ShipmentState
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string ShipmentNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public Guid? CarrierId { get; set; }
    public string? CarrierName { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public string ChecklistStatus { get; set; } = string.Empty;
    public bool HasOccurrence { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
}

public sealed class FinanceEntryState
{
    public Guid Id { get; init; }
    public Guid? OrderId { get; init; }
    public string? OrderNumber { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Counterparty { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime DueAtUtc { get; set; }
    public string EntrySource { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? BoletoStatus { get; set; }
    public string? BoletoNumber { get; set; }
    public string? BoletoLine { get; set; }
}

public sealed class RegisterEntryState
{
    public Guid Id { get; init; }
    public string GroupKey { get; init; } = string.Empty;
    public string GroupLabel { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class ProductMaterialRequirementState
{
    public Guid MaterialId { get; init; }
    public string MaterialName { get; init; } = string.Empty;
    public decimal QuantityPerUnit { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public sealed class ProductTemplateState
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BillingMethod { get; set; } = string.Empty;
    public decimal DefaultUnitPrice { get; set; }
    public string DefaultProductionSector { get; set; } = string.Empty;
    public string FiscalNcm { get; set; } = "8208.90.00";
    public string FiscalCfop { get; set; } = "5101";
    public string FiscalCommercialUnit { get; set; } = "UN";
    public string FiscalOriginCode { get; set; } = "0";
    public string FiscalIcmsSituationCode { get; set; } = "00";
    public string FiscalIpiSituationCode { get; set; } = "99";
    public string FiscalPisSituationCode { get; set; } = "49";
    public string FiscalCofinsSituationCode { get; set; } = "49";
    public decimal FiscalIcmsRate { get; set; }
    public decimal FiscalIpiRate { get; set; }
    public decimal FiscalPisRate { get; set; }
    public decimal FiscalCofinsRate { get; set; }
    public bool Active { get; set; }
    public List<ProductMaterialRequirementState> MaterialRequirements { get; set; } = [];
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class CarrierState
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string BusinessHours { get; set; } = string.Empty;
    public string ServiceArea { get; set; } = string.Empty;
    public string DefaultMode { get; set; } = string.Empty;
    public bool DoesPickup { get; set; }
    public bool DoesDelivery { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class FiscalInvoiceState
{
    public Guid Id { get; init; }
    public Guid? FinanceEntryId { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string EngineName { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
    public string CertificateMedia { get; set; } = string.Empty;
    public string NatureOfOperation { get; set; } = string.Empty;
    public string Cfop { get; set; } = string.Empty;
    public string? XmlArchivePath { get; set; }
    public string? DanfeArchivePath { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class FiscalAddressSnapshotState
{
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string? CityIbgeCode { get; set; }
    public string Country { get; set; } = "Brasil";
    public string? Complement { get; set; }
    public string? ReferencePoint { get; set; }
}

public sealed class FiscalEmitterSnapshotState
{
    public Guid CompanyId { get; set; }
    public string TradeName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string StateRegistration { get; set; } = string.Empty;
    public string TaxRegime { get; set; } = string.Empty;
    public string FiscalSeries { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public FiscalAddressSnapshotState Address { get; set; } = new();
}

public sealed class FiscalRecipientSnapshotState
{
    public Guid? CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? StateRegistration { get; set; }
    public string TaxpayerIndicator { get; set; } = "NaoContribuinte";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public FiscalAddressSnapshotState Address { get; set; } = new();
}

public sealed class FiscalDocumentItemState
{
    public int LineNumber { get; set; }
    public Guid? ProductTemplateId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CommercialUnit { get; set; } = "UN";
    public decimal Quantity { get; set; }
    public decimal TaxQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? BillingMethod { get; set; }
    public string Cfop { get; set; } = string.Empty;
    public string Ncm { get; set; } = string.Empty;
    public string OriginCode { get; set; } = "0";
    public string IcmsSituationCode { get; set; } = "00";
    public string IpiSituationCode { get; set; } = "99";
    public string PisSituationCode { get; set; } = "49";
    public string CofinsSituationCode { get; set; } = "49";
    public decimal IcmsRate { get; set; }
    public decimal IcmsBaseAmount { get; set; }
    public decimal IcmsAmount { get; set; }
    public decimal IpiRate { get; set; }
    public decimal IpiAmount { get; set; }
    public decimal PisRate { get; set; }
    public decimal PisAmount { get; set; }
    public decimal CofinsRate { get; set; }
    public decimal CofinsAmount { get; set; }
    public string? Notes { get; set; }
}

public sealed class FiscalDocumentTotalsState
{
    public decimal ProductsAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FreightAmount { get; set; }
    public decimal InsuranceAmount { get; set; }
    public decimal OtherAmount { get; set; }
    public decimal IcmsBaseAmount { get; set; }
    public decimal IcmsAmount { get; set; }
    public decimal IpiAmount { get; set; }
    public decimal PisAmount { get; set; }
    public decimal CofinsAmount { get; set; }
    public decimal InvoiceAmount { get; set; }
}

public sealed class FiscalDocumentPaymentState
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string BillingType { get; set; } = "A vista";
    public string? EntrySource { get; set; }
    public decimal BillingAmount { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public string? BoletoNumber { get; set; }
    public string? BoletoLine { get; set; }
}

public sealed class FiscalDocumentTransportState
{
    public Guid? ShipmentId { get; set; }
    public Guid? CarrierId { get; set; }
    public string? CarrierName { get; set; }
    public string Mode { get; set; } = "Sem frete";
    public string FreightMode { get; set; } = "Sem frete";
    public string? RecipientName { get; set; }
    public string? DriverName { get; set; }
    public string? VehiclePlate { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
}

public sealed class FiscalDocumentState
{
    public Guid Id { get; init; }
    public Guid CompanyProfileId { get; init; }
    public Guid? FinanceEntryId { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string? Protocol { get; set; }
    public string AdapterName { get; set; } = string.Empty;
    public string IssueMode { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
    public string CertificateMedia { get; set; } = string.Empty;
    public string NatureOfOperation { get; set; } = string.Empty;
    public string Cfop { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string? RecipientDocument { get; set; }
    public decimal Amount { get; set; }
    public FiscalEmitterSnapshotState EmitterSnapshot { get; set; } = new();
    public FiscalRecipientSnapshotState RecipientSnapshot { get; set; } = new();
    public List<FiscalDocumentItemState> Items { get; set; } = [];
    public FiscalDocumentTotalsState Totals { get; set; } = new();
    public FiscalDocumentPaymentState Payment { get; set; } = new();
    public FiscalDocumentTransportState Transport { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string? LastError { get; set; }
    public int AttemptsCount { get; set; }
    public string? XmlArchivePath { get; set; }
    public string? DanfeArchivePath { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? IssuedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class FiscalOperationTemplateState
{
    public Guid Id { get; init; }
    public Guid? CompanyProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NatureOfOperation { get; set; } = string.Empty;
    public string Cfop { get; set; } = string.Empty;
    public string Finality { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string? Notes { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class FiscalTransmissionAttemptState
{
    public Guid Id { get; init; }
    public Guid FiscalDocumentId { get; init; }
    public int AttemptNumber { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string AdapterName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ResponseCode { get; set; }
    public string? ResponseSummary { get; set; }
    public DateTime AttemptedAtUtc { get; set; }
}

public sealed class FiscalEventState
{
    public Guid Id { get; init; }
    public Guid FiscalDocumentId { get; init; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
    public Guid? ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
}

public sealed class FiscalArtifactState
{
    public Guid Id { get; init; }
    public Guid FiscalDocumentId { get; init; }
    public string Kind { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class FiscalAgentRegistrationState
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string CertificateMedia { get; set; } = string.Empty;
    public bool Online { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public sealed class FiscalNumberingEventState
{
    public Guid Id { get; init; }
    public Guid CompanyProfileId { get; set; }
    public string Series { get; set; } = string.Empty;
    public int StartNumber { get; set; }
    public int EndNumber { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string AdapterName { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? XmlArchivePath { get; set; }
    public string? PreviewArchivePath { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class FiscalCompanyProfileState
{
    public Guid Id { get; init; }
    public string TradeName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string StateRegistration { get; set; } = string.Empty;
    public string TaxRegime { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string CityIbgeCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string FiscalSeries { get; set; } = string.Empty;
    public bool NfeEnabled { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string AdapterName { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
    public string CertificateMedia { get; set; } = string.Empty;
    public string? CertificateLabel { get; set; }
    public string? CertificateSerialNumber { get; set; }
    public string PrincipalEmissionMode { get; set; } = string.Empty;
    public string? ContingencyEmissionMode { get; set; }
    public bool AccountantValidated { get; set; }
    public bool HomologationCredentialsValidated { get; set; }
    public bool HomologationApproved { get; set; }
    public bool ProductionCredentialsValidated { get; set; }
    public bool ProductionApproved { get; set; }
    public string? OnboardingNotes { get; set; }
    public int LastNfeNumber { get; set; }
}

public sealed class TechnicalAssetState
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public List<string> Components { get; set; } = [];
    public List<string> Materials { get; set; } = [];
    public string? LastOrderNumber { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
}
