using PackControl.Domain.Identity;
using PackControl.Domain.Orders;

namespace PackControl.Infrastructure.Persistence;

public sealed class AppStateSnapshot
{
    public List<AppUserSnapshot> Users { get; set; } = [];
    public List<CustomerSnapshot> Customers { get; set; } = [];
    public List<OrderSnapshot> Orders { get; set; } = [];
    public List<AuditLogSnapshot> AuditLogs { get; set; } = [];
    public List<ProductionOrderState> ProductionOrders { get; set; } = [];
    public List<MaterialCatalogItemState> Materials { get; set; } = [];
    public List<StockItemState> StockItems { get; set; } = [];
    public List<ShipmentState> Shipments { get; set; } = [];
    public List<FinanceEntryState> FinanceEntries { get; set; } = [];
    public List<RegisterEntryState> RegisterEntries { get; set; } = [];
    public List<ProductTemplateState> ProductTemplates { get; set; } = [];
    public List<CarrierState> Carriers { get; set; } = [];
    public List<FiscalInvoiceState> FiscalInvoices { get; set; } = [];
    public List<FiscalDocumentState> FiscalDocuments { get; set; } = [];
    public List<FiscalOperationTemplateState> FiscalOperationTemplates { get; set; } = [];
    public List<FiscalTransmissionAttemptState> FiscalTransmissionAttempts { get; set; } = [];
    public List<FiscalEventState> FiscalEvents { get; set; } = [];
    public List<FiscalArtifactState> FiscalArtifacts { get; set; } = [];
    public List<FiscalAgentRegistrationState> FiscalAgents { get; set; } = [];
    public List<FiscalNumberingEventState> FiscalNumberingEvents { get; set; } = [];
    public List<FiscalCompanyProfileState> FiscalCompanies { get; set; } = [];
    public List<TechnicalAssetState> TechnicalAssets { get; set; } = [];
}

public sealed class AppUserSnapshot
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class CustomerProductPricingRuleSnapshot
{
    public Guid ProductTemplateId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BillingMethod { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}

public sealed class CustomerSnapshot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public List<string> Nicknames { get; set; } = [];
    public string? PostalCode { get; set; }
    public string? Street { get; set; }
    public string? StreetNumber { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? CityIbgeCode { get; set; }
    public string? StateRegistration { get; set; }
    public string TaxpayerIndicator { get; set; } = "NaoContribuinte";
    public string? Complement { get; set; }
    public string? ReferencePoint { get; set; }
    public Guid? DefaultCarrierId { get; set; }
    public string? DefaultCarrierName { get; set; }
    public string? DefaultDeliveryMode { get; set; }
    public List<CustomerProductPricingRuleSnapshot> ProductPricingRules { get; set; } = [];
    public int Score { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class OrderScopeItemSnapshot
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid? ProductTemplateId { get; set; }
    public string? ProductName { get; set; }
    public string? BillingMethod { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class OrderAttachmentSnapshot
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class TechnicalAnalysisSnapshot
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid AttachmentId { get; set; }
    public string SourceFileExtension { get; set; } = string.Empty;
    public TechnicalAnalysisStatus Status { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? EngineName { get; set; }
    public int? ConfidencePercent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class OrderSnapshot
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public ServiceType ServiceType { get; set; }
    public UrgencyLevel Urgency { get; set; }
    public OrderStatus Status { get; set; }
    public string? ContextSummary { get; set; }
    public string? LegacyAssetReference { get; set; }
    public string? Notes { get; set; }
    public List<OrderScopeItemSnapshot> ScopeItems { get; set; } = [];
    public List<OrderAttachmentSnapshot> Attachments { get; set; } = [];
    public List<TechnicalAnalysisSnapshot> Analyses { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class AuditLogSnapshot
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
