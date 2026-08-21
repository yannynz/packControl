using PackControl.Domain.Audit;
using PackControl.Domain.Customers;
using PackControl.Domain.Identity;
using PackControl.Domain.Orders;

namespace PackControl.Infrastructure.Persistence;

public sealed class AppStateStore
{
    public object SyncRoot { get; } = new();
    public List<AppUser> Users { get; } = [];
    public List<Customer> Customers { get; } = [];
    public List<Order> Orders { get; } = [];
    public List<AuditLog> AuditLogs { get; } = [];
    public List<ProductionOrderState> ProductionOrders { get; } = [];
    public List<MaterialCatalogItemState> Materials { get; } = [];
    public List<StockItemState> StockItems { get; } = [];
    public List<ShipmentState> Shipments { get; } = [];
    public List<FinanceEntryState> FinanceEntries { get; } = [];
    public List<RegisterEntryState> RegisterEntries { get; } = [];
    public List<ProductTemplateState> ProductTemplates { get; } = [];
    public List<CarrierState> Carriers { get; } = [];
    public List<FiscalInvoiceState> FiscalInvoices { get; } = [];
    public List<FiscalDocumentState> FiscalDocuments { get; } = [];
    public List<FiscalOperationTemplateState> FiscalOperationTemplates { get; } = [];
    public List<FiscalTransmissionAttemptState> FiscalTransmissionAttempts { get; } = [];
    public List<FiscalEventState> FiscalEvents { get; } = [];
    public List<FiscalArtifactState> FiscalArtifacts { get; } = [];
    public List<FiscalAgentRegistrationState> FiscalAgents { get; } = [];
    public List<FiscalNumberingEventState> FiscalNumberingEvents { get; } = [];
    public List<FiscalCompanyProfileState> FiscalCompanies { get; } = [];
    public List<TechnicalAssetState> TechnicalAssets { get; } = [];

    public AppStateSnapshot ToSnapshot() => new()
    {
        Users = Users.Select(user => new AppUserSnapshot
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            CreatedBy = user.CreatedBy,
            UpdatedAtUtc = user.UpdatedAtUtc,
            UpdatedBy = user.UpdatedBy
        }).ToList(),
        Customers = Customers.Select(customer => new CustomerSnapshot
        {
            Id = customer.Id,
            Name = customer.Name,
            DocumentNumber = customer.DocumentNumber,
            ContactName = customer.ContactName,
            Email = customer.Email,
            Phone = customer.Phone,
            Notes = customer.Notes,
            Nicknames = [.. customer.Nicknames],
            PostalCode = customer.PostalCode,
            Street = customer.Street,
            StreetNumber = customer.StreetNumber,
            District = customer.District,
            City = customer.City,
            State = customer.State,
            CityIbgeCode = customer.CityIbgeCode,
            StateRegistration = customer.StateRegistration,
            TaxpayerIndicator = customer.TaxpayerIndicator,
            Complement = customer.Complement,
            ReferencePoint = customer.ReferencePoint,
            DefaultCarrierId = customer.DefaultCarrierId,
            DefaultCarrierName = customer.DefaultCarrierName,
            DefaultDeliveryMode = customer.DefaultDeliveryMode,
            ProductPricingRules = customer.ProductPricingRules
                .Select(rule => new CustomerProductPricingRuleSnapshot
                {
                    ProductTemplateId = rule.ProductTemplateId,
                    ProductName = rule.ProductName,
                    BillingMethod = rule.BillingMethod,
                    UnitPrice = rule.UnitPrice,
                    Notes = rule.Notes
                })
                .ToList(),
            Score = customer.Score,
            CreatedAtUtc = customer.CreatedAtUtc,
            CreatedBy = customer.CreatedBy,
            UpdatedAtUtc = customer.UpdatedAtUtc,
            UpdatedBy = customer.UpdatedBy
        }).ToList(),
        Orders = Orders.Select(order => new OrderSnapshot
        {
            Id = order.Id,
            Number = order.Number,
            CustomerId = order.CustomerId,
            ServiceType = order.ServiceType,
            Urgency = order.Urgency,
            Status = order.Status,
            ContextSummary = order.ContextSummary,
            LegacyAssetReference = order.LegacyAssetReference,
            Notes = order.Notes,
            ScopeItems = order.ScopeItems.Select(item => new OrderScopeItemSnapshot
            {
                Id = item.Id,
                OrderId = item.OrderId,
                Title = item.Title,
                Category = item.Category,
                Quantity = item.Quantity,
                ProductTemplateId = item.ProductTemplateId,
                ProductName = item.ProductName,
                BillingMethod = item.BillingMethod,
                UnitPrice = item.UnitPrice,
                Notes = item.Notes,
                CreatedAtUtc = item.CreatedAtUtc,
                CreatedBy = item.CreatedBy,
                UpdatedAtUtc = item.UpdatedAtUtc,
                UpdatedBy = item.UpdatedBy
            }).ToList(),
            Attachments = order.Attachments.Select(attachment => new OrderAttachmentSnapshot
            {
                Id = attachment.Id,
                OrderId = attachment.OrderId,
                OriginalFileName = attachment.OriginalFileName,
                StoredFileName = attachment.StoredFileName,
                StoragePath = attachment.StoragePath,
                ContentType = attachment.ContentType,
                SizeBytes = attachment.SizeBytes,
                Sha256 = attachment.Sha256,
                CreatedAtUtc = attachment.CreatedAtUtc,
                CreatedBy = attachment.CreatedBy,
                UpdatedAtUtc = attachment.UpdatedAtUtc,
                UpdatedBy = attachment.UpdatedBy
            }).ToList(),
            Analyses = order.Analyses.Select(analysis => new TechnicalAnalysisSnapshot
            {
                Id = analysis.Id,
                OrderId = analysis.OrderId,
                AttachmentId = analysis.AttachmentId,
                SourceFileExtension = analysis.SourceFileExtension,
                Status = analysis.Status,
                Summary = analysis.Summary,
                EngineName = analysis.EngineName,
                ConfidencePercent = analysis.ConfidencePercent,
                CreatedAtUtc = analysis.CreatedAtUtc,
                CreatedBy = analysis.CreatedBy,
                UpdatedAtUtc = analysis.UpdatedAtUtc,
                UpdatedBy = analysis.UpdatedBy
            }).ToList(),
            CreatedAtUtc = order.CreatedAtUtc,
            CreatedBy = order.CreatedBy,
            UpdatedAtUtc = order.UpdatedAtUtc,
            UpdatedBy = order.UpdatedBy
        }).ToList(),
        AuditLogs = AuditLogs.Select(log => new AuditLogSnapshot
        {
            Id = log.Id,
            ActorUserId = log.ActorUserId,
            ActorName = log.ActorName,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            Action = log.Action,
            Description = log.Description,
            MetadataJson = log.MetadataJson,
            OccurredAtUtc = log.OccurredAtUtc
        }).ToList(),
        ProductionOrders = [.. ProductionOrders],
        Materials = [.. Materials],
        StockItems = [.. StockItems],
        Shipments = [.. Shipments],
        FinanceEntries = [.. FinanceEntries],
        RegisterEntries = [.. RegisterEntries],
        ProductTemplates = [.. ProductTemplates],
        Carriers = [.. Carriers],
        FiscalInvoices = [.. FiscalInvoices],
        FiscalDocuments = [.. FiscalDocuments],
        FiscalOperationTemplates = [.. FiscalOperationTemplates],
        FiscalTransmissionAttempts = [.. FiscalTransmissionAttempts],
        FiscalEvents = [.. FiscalEvents],
        FiscalArtifacts = [.. FiscalArtifacts],
        FiscalAgents = [.. FiscalAgents],
        FiscalNumberingEvents = [.. FiscalNumberingEvents],
        FiscalCompanies = [.. FiscalCompanies],
        TechnicalAssets = [.. TechnicalAssets]
    };

    public void LoadSnapshot(AppStateSnapshot snapshot)
    {
        ReplaceList(Users, snapshot.Users.Select(user => AppUser.Restore(
            user.Id,
            user.Email,
            user.FullName,
            user.PasswordHash,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc,
            user.CreatedBy,
            user.UpdatedAtUtc,
            user.UpdatedBy)));
        ReplaceList(Customers, snapshot.Customers.Select(customer => Customer.Restore(
            customer.Id,
            customer.Name,
            customer.DocumentNumber,
            customer.ContactName,
            customer.Email,
            customer.Phone,
            customer.Notes,
            customer.Nicknames,
            customer.PostalCode,
            customer.Street,
            customer.StreetNumber,
            customer.District,
            customer.City,
            customer.State,
            customer.CityIbgeCode,
            customer.StateRegistration,
            customer.TaxpayerIndicator,
            customer.Complement,
            customer.ReferencePoint,
            customer.DefaultCarrierId,
            customer.DefaultCarrierName,
            customer.DefaultDeliveryMode,
            customer.ProductPricingRules.Select(rule => CustomerProductPricingRule.Restore(
                rule.ProductTemplateId,
                rule.ProductName,
                rule.BillingMethod,
                rule.UnitPrice,
                rule.Notes)),
            customer.Score,
            customer.CreatedAtUtc,
            customer.CreatedBy,
            customer.UpdatedAtUtc,
            customer.UpdatedBy)));
        ReplaceList(Orders, snapshot.Orders.Select(order => Order.Restore(
            order.Id,
            order.Number,
            order.CustomerId,
            order.ServiceType,
            order.Urgency,
            order.Status,
            order.ContextSummary,
            order.LegacyAssetReference,
            order.Notes,
            order.ScopeItems.Select(item => OrderScopeItem.Restore(
                item.Id,
                item.OrderId,
                item.Title,
                item.Category,
                item.Quantity,
                item.ProductTemplateId,
                item.ProductName,
                item.BillingMethod,
                item.UnitPrice,
                item.Notes,
                item.CreatedAtUtc,
                item.CreatedBy,
                item.UpdatedAtUtc,
                item.UpdatedBy)),
            order.Attachments.Select(attachment => OrderAttachment.Restore(
                attachment.Id,
                attachment.OrderId,
                attachment.OriginalFileName,
                attachment.StoredFileName,
                attachment.StoragePath,
                attachment.ContentType,
                attachment.SizeBytes,
                attachment.Sha256,
                attachment.CreatedAtUtc,
                attachment.CreatedBy,
                attachment.UpdatedAtUtc,
                attachment.UpdatedBy)),
            order.Analyses.Select(analysis => TechnicalAnalysis.Restore(
                analysis.Id,
                analysis.OrderId,
                analysis.AttachmentId,
                analysis.SourceFileExtension,
                analysis.Status,
                analysis.Summary,
                analysis.EngineName,
                analysis.ConfidencePercent,
                analysis.CreatedAtUtc,
                analysis.CreatedBy,
                analysis.UpdatedAtUtc,
                analysis.UpdatedBy)),
            order.CreatedAtUtc,
            order.CreatedBy,
            order.UpdatedAtUtc,
            order.UpdatedBy)));
        ReplaceList(AuditLogs, snapshot.AuditLogs.Select(log => AuditLog.Restore(
            log.Id,
            log.ActorUserId,
            log.ActorName,
            log.EntityName,
            log.EntityId,
            log.Action,
            log.Description,
            log.MetadataJson,
            log.OccurredAtUtc)));
        ReplaceList(ProductionOrders, snapshot.ProductionOrders);
        ReplaceList(Materials, snapshot.Materials);
        ReplaceList(StockItems, snapshot.StockItems);
        ReplaceList(Shipments, snapshot.Shipments);
        ReplaceList(FinanceEntries, snapshot.FinanceEntries);
        ReplaceList(RegisterEntries, snapshot.RegisterEntries);
        ReplaceList(ProductTemplates, snapshot.ProductTemplates);
        ReplaceList(Carriers, snapshot.Carriers);
        ReplaceList(FiscalInvoices, snapshot.FiscalInvoices);
        ReplaceList(FiscalDocuments, snapshot.FiscalDocuments);
        ReplaceList(FiscalOperationTemplates, snapshot.FiscalOperationTemplates);
        ReplaceList(FiscalTransmissionAttempts, snapshot.FiscalTransmissionAttempts);
        ReplaceList(FiscalEvents, snapshot.FiscalEvents);
        ReplaceList(FiscalArtifacts, snapshot.FiscalArtifacts);
        ReplaceList(FiscalAgents, snapshot.FiscalAgents);
        ReplaceList(FiscalNumberingEvents, snapshot.FiscalNumberingEvents);
        ReplaceList(FiscalCompanies, snapshot.FiscalCompanies);
        ReplaceList(TechnicalAssets, snapshot.TechnicalAssets);
    }

    private static void ReplaceList<T>(List<T> target, IEnumerable<T>? source)
    {
        target.Clear();
        if (source is not null)
        {
            target.AddRange(source);
        }
    }
}
