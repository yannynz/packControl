using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Orders;
using PackControl.Domain.Audit;
using PackControl.Domain.Customers;
using PackControl.Domain.Orders;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class OrderService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IFileStorage fileStorage,
    IAppStatePersistence statePersistence,
    TechnicalDocumentAnalyzer technicalDocumentAnalyzer) : IOrderService
{
    public async Task<IReadOnlyList<OrderListItemDto>> ListAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return (
                from order in stateStore.Orders
                join customer in stateStore.Customers on order.CustomerId equals customer.Id
                orderby order.CreatedAtUtc descending
                select new OrderListItemDto(
                    order.Id,
                    order.Number,
                    customer.Name,
                    order.Status.ToString(),
                    order.ServiceType.ToString(),
                    order.Urgency.ToString(),
                    BuildScopePreview(order),
                    order.ScopeItems.Count,
                    order.Attachments.Count,
                    order.CreatedAtUtc))
                .ToList();
        }
    }

    public async Task<OrderDetailDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            var order = stateStore.Orders.SingleOrDefault(x => x.Id == orderId);
            if (order is null)
            {
                return null;
            }

            var customerName = stateStore.Customers.Single(x => x.Id == order.CustomerId).Name;
            return Map(order, customerName, stateStore);
        }
    }

    public async Task<OrderDetailDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var actor = currentUserAccessor.DisplayName;
        OrderDetailDto detail;
        lock (stateStore.SyncRoot)
        {
            var customer = stateStore.Customers.SingleOrDefault(x => x.Id == request.CustomerId);
            if (customer is null)
            {
                throw new InvalidOperationException("Cliente informado nao existe.");
            }

            if (request.ScopeItems.Count == 0)
            {
                throw new InvalidOperationException("O pedido precisa de pelo menos um item de escopo.");
            }

            var order = Order.Create(
                GenerateOrderNumber(),
                request.CustomerId,
                request.ServiceType,
                request.Urgency,
                request.ContextSummary,
                request.LegacyAssetReference,
                request.Notes,
                clock.UtcNow,
                actor);

            foreach (var item in request.ScopeItems)
            {
                var productTemplate = item.ProductTemplateId is null
                    ? null
                    : stateStore.ProductTemplates.SingleOrDefault(x => x.Id == item.ProductTemplateId.Value);
                var customerPricingRule = item.ProductTemplateId is null
                    ? null
                    : customer.ProductPricingRules.SingleOrDefault(x => x.ProductTemplateId == item.ProductTemplateId.Value);

                if (item.ProductTemplateId is not null && productTemplate is null)
                {
                    throw new InvalidOperationException("Produto comercial informado nao existe.");
                }

                var title = string.IsNullOrWhiteSpace(item.Title) ? productTemplate?.Name : item.Title;
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new InvalidOperationException("Cada item de escopo precisa de um titulo ou produto comercial.");
                }

                var category = string.IsNullOrWhiteSpace(item.Category) ? productTemplate?.Category : item.Category;
                if (string.IsNullOrWhiteSpace(category))
                {
                    throw new InvalidOperationException("Cada item de escopo precisa de uma categoria.");
                }

                order.AddScopeItem(
                    title,
                    category!,
                    item.Quantity,
                    productTemplate?.Id,
                    string.IsNullOrWhiteSpace(item.ProductName) ? productTemplate?.Name : item.ProductName,
                    string.IsNullOrWhiteSpace(item.BillingMethod)
                        ? customerPricingRule?.BillingMethod ?? productTemplate?.BillingMethod
                        : item.BillingMethod,
                    item.UnitPrice ?? customerPricingRule?.UnitPrice ?? productTemplate?.DefaultUnitPrice,
                    item.Notes,
                    clock.UtcNow,
                    actor);
            }

            stateStore.Orders.Add(order);
            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                actor,
                nameof(Order),
                order.Id,
                "order.created",
                $"Pedido {order.Number} criado com {order.ScopeItems.Count} item(ns) de escopo.",
                JsonSerializer.Serialize(new
                {
                    request.ServiceType,
                    request.Urgency,
                    request.ContextSummary,
                    request.LegacyAssetReference
                }),
                clock.UtcNow));

            var customerName = customer.Name;
            detail = Map(order, customerName, stateStore);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return detail;
    }

    public async Task<OrderDetailDto?> AttachFileAsync(
        Guid orderId,
        Stream fileStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var storedFile = await fileStorage.SaveAsync(fileStream, fileName, contentType, cancellationToken);
        var actor = currentUserAccessor.DisplayName;
        OrderDetailDto? detail;
        Guid? analysisId = null;
        var extension = Path.GetExtension(fileName).Trim().ToLowerInvariant();
        lock (stateStore.SyncRoot)
        {
            var order = stateStore.Orders.SingleOrDefault(x => x.Id == orderId);
            if (order is null)
            {
                return null;
            }

            var attachment = order.AddAttachment(
                storedFile.OriginalFileName,
                storedFile.StoredFileName,
                storedFile.StoragePath,
                storedFile.ContentType,
                storedFile.SizeBytes,
                storedFile.Sha256,
                clock.UtcNow,
                actor);

            if (extension is ".pdf" or ".dxf")
            {
                var summary = extension switch
                {
                    ".pdf" => "Arquivo PDF recebido. Analise documental em processamento.",
                    ".dxf" => "Arquivo DXF recebido. Analise geometrica em processamento.",
                    _ => "Analise pendente."
                };

                analysisId = order.RegisterPendingAnalysis(attachment.Id, extension, summary, clock.UtcNow, actor).Id;
            }

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                actor,
                nameof(Order),
                order.Id,
                "order.attachment_added",
                $"Arquivo {fileName} anexado ao pedido {order.Number}.",
                JsonSerializer.Serialize(new { storedFile.ContentType, storedFile.SizeBytes }),
                clock.UtcNow));

            var customerName = stateStore.Customers.Single(x => x.Id == order.CustomerId).Name;
            detail = Map(order, customerName, stateStore);
        }

        if (analysisId is not null)
        {
            await using var analysisStream = await fileStorage.OpenReadAsync(storedFile.StoragePath, cancellationToken);
            var analysisResult = await technicalDocumentAnalyzer.AnalyzeAsync(extension, analysisStream, cancellationToken);

            lock (stateStore.SyncRoot)
            {
                var order = stateStore.Orders.SingleOrDefault(x => x.Id == orderId);
                var analysis = order?.Analyses.SingleOrDefault(x => x.Id == analysisId.Value);
                if (order is not null && analysis is not null)
                {
                    if (analysisResult.Success)
                    {
                        analysis.Complete(
                            analysisResult.Summary,
                            analysisResult.EngineName,
                            analysisResult.ConfidencePercent,
                            clock.UtcNow,
                            actor);
                    }
                    else
                    {
                        analysis.Fail(
                            analysisResult.Summary,
                            analysisResult.EngineName,
                            clock.UtcNow,
                            actor);
                    }

                    var customerName = stateStore.Customers.Single(x => x.Id == order.CustomerId).Name;
                    detail = Map(order, customerName, stateStore);
                }
            }
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return detail;
    }

    public async Task<OrderDetailDto?> ApproveAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var actor = currentUserAccessor.DisplayName;
        OrderDetailDto? detail;

        lock (stateStore.SyncRoot)
        {
            var order = stateStore.Orders.SingleOrDefault(x => x.Id == orderId);
            if (order is null)
            {
                return null;
            }

            if (order.Status != OrderStatus.Approved && order.Status != OrderStatus.InProduction)
            {
                order.Approve(clock.UtcNow, actor);
            }

            var customer = stateStore.Customers.Single(x => x.Id == order.CustomerId);
            EnsureOperationalArtifacts(order, customer, actor);

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                actor,
                nameof(Order),
                order.Id,
                "order.approved",
                $"Pedido {order.Number} aprovado e encaminhado para producao, logistica e financeiro.",
                JsonSerializer.Serialize(new
                {
                    ScopeItemsCount = order.ScopeItems.Count,
                    AttachmentsCount = order.Attachments.Count
                }),
                clock.UtcNow));

            detail = Map(order, customer.Name, stateStore);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return detail;
    }

    private void EnsureOperationalArtifacts(Order order, Customer customer, string actor)
    {
        var customerName = customer.Name;

        if (!stateStore.ProductionOrders.Any(x => x.OrderId == order.Id))
        {
            var index = 1;
            foreach (var scopeItem in order.ScopeItems)
            {
                var productionNumber = $"OP-{order.Number}-{index:00}";
                var template = scopeItem.ProductTemplateId is null
                    ? null
                    : stateStore.ProductTemplates.SingleOrDefault(x => x.Id == scopeItem.ProductTemplateId.Value);
                var sector = template?.DefaultProductionSector ?? ResolveInitialSector(scopeItem.Category);

                stateStore.ProductionOrders.Add(new ProductionOrderState
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    OrderNumber = order.Number,
                    Number = productionNumber,
                    VisibleInQueues = true,
                    ParentProductionOrderId = null,
                    ParentProductionOrderNumber = null,
                    MergedIntoProductionOrderId = null,
                    MergedIntoProductionOrderNumber = null,
                    RelatedProductionOrderNumbers = [],
                    TraceabilityReason = null,
                    CustomerName = customerName,
                    Title = scopeItem.Title,
                    Quantity = scopeItem.Quantity,
                    ProductTemplateId = scopeItem.ProductTemplateId,
                    ProductName = scopeItem.ProductName,
                    BillingMethod = scopeItem.BillingMethod ?? "Sob consulta",
                    UnitPrice = scopeItem.UnitPrice,
                    Sector = sector,
                    Status = "Aguardando fila",
                    Priority = ResolvePriority(order.Urgency),
                    Owner = ResolveSectorOwner(sector),
                    Complexity = ResolveComplexity(scopeItem.Quantity, scopeItem.Category),
                    Outsourced = scopeItem.Category is "acessorio",
                    MaterialSupport = ResolveMaterialSupport(scopeItem.Category, template),
                    DueAtUtc = clock.UtcNow.AddDays(3 + index),
                    UpdatedAtUtc = clock.UtcNow
                });

                ApplyAutomaticStockConsumption(scopeItem, template, productionNumber, actor);
                index++;
            }
        }

        if (!stateStore.Shipments.Any(x => x.OrderId == order.Id))
        {
            var carrier = customer.DefaultCarrierId is null
                ? null
                : stateStore.Carriers.SingleOrDefault(x => x.Id == customer.DefaultCarrierId.Value);
            var mode = customer.DefaultDeliveryMode ?? carrier?.DefaultMode ?? "Entrega propria";

            stateStore.Shipments.Add(new ShipmentState
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OrderNumber = order.Number,
                ShipmentNumber = $"LOT-{order.Number}",
                CustomerName = customerName,
                Mode = mode,
                Status = "Aguardando producao",
                Recipient = customerName,
                CarrierId = carrier?.Id,
                CarrierName = customer.DefaultCarrierName ?? carrier?.Name,
                DriverName = carrier is null ? "Equipe interna" : carrier.ContactName,
                VehiclePlate = carrier is null ? "BRA-2E19" : "TER-0000",
                ChecklistStatus = "Pendente",
                HasOccurrence = false,
                ScheduledAtUtc = clock.UtcNow.AddDays(5)
            });
        }

        if (!stateStore.FinanceEntries.Any(x => x.OrderId == order.Id))
        {
            var baseAmount = order.ScopeItems.Sum(ResolveEstimatedRevenue);
            stateStore.FinanceEntries.Add(new FinanceEntryState
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OrderNumber = order.Number,
                Type = "Receber",
                Status = "Em aberto",
                Description = $"Recebimento comercial do pedido {order.Number}",
                Counterparty = customerName,
                Amount = baseAmount,
                DueAtUtc = clock.UtcNow.AddDays(14),
                EntrySource = "Pedido",
                PaymentMethod = ResolveFinancePaymentMethod(order),
                Notes = $"Projetado automaticamente a partir das OPs do pedido {order.Number}."
            });

            stateStore.FinanceEntries.Add(new FinanceEntryState
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OrderNumber = order.Number,
                Type = "Pagar",
                Status = "Programado",
                Description = $"Consumo previsto de materiais para {order.Number}",
                Counterparty = "Fornecedor principal",
                Amount = decimal.Round(baseAmount * 0.34m, 2),
                DueAtUtc = clock.UtcNow.AddDays(7),
                EntrySource = "Pedido",
                PaymentMethod = "Compra programada",
                Notes = "Lancamento estimado de suprimentos."
            });
        }

        stateStore.AuditLogs.Add(AuditLog.Create(
            currentUserAccessor.UserId,
            actor,
            nameof(Order),
            order.Id,
            "order.operational_projection_created",
            "Projecoes de producao, logistica e financeiro atualizadas.",
            null,
            clock.UtcNow));
    }

    private string GenerateOrderNumber()
    {
        var count = stateStore.Orders.Count + 1;
        return $"PED-{clock.UtcNow:yyyyMMdd}-{count:000}";
    }

    private static string BuildScopePreview(Order order)
    {
        var labels = order.ScopeItems
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => x.Title)
            .Take(2)
            .ToList();

        if (labels.Count == 0)
        {
            return "Sem itens de escopo";
        }

        var preview = string.Join(" + ", labels);
        return order.ScopeItems.Count > 2 ? $"{preview} +{order.ScopeItems.Count - 2}" : preview;
    }

    private void ApplyAutomaticStockConsumption(
        OrderScopeItem scopeItem,
        ProductTemplateState? template,
        string productionNumber,
        string actor)
    {
        foreach (var requirement in ResolveMaterialRequirements(scopeItem, template))
        {
            var stockItem = stateStore.StockItems.SingleOrDefault(x => x.MaterialId == requirement.MaterialId);
            if (stockItem is null)
            {
                continue;
            }

            var consumed = decimal.Round(Math.Max(0, requirement.QuantityPerUnit * scopeItem.Quantity), 2);
            stockItem.OnHand = Math.Max(0, stockItem.OnHand - consumed);
            stockItem.LastMovement = $"Baixa automatica da {productionNumber} ({consumed:0.##} {requirement.Unit}).";
            stockItem.LastMovementAtUtc = clock.UtcNow;

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                actor,
                "Stock",
                stockItem.Id,
                "stock.auto_consumed",
                $"Baixa automatica aplicada em {stockItem.MaterialName} para {productionNumber}.",
                JsonSerializer.Serialize(new { productionNumber, consumed }),
                clock.UtcNow));
        }
    }

    private IEnumerable<ProductMaterialRequirementState> ResolveMaterialRequirements(
        OrderScopeItem scopeItem,
        ProductTemplateState? template)
    {
        if (template?.MaterialRequirements.Count > 0)
        {
            return template.MaterialRequirements;
        }

        return scopeItem.Category switch
        {
            "produto_principal" => BuildFallbackRequirements(("aco", 1m, "chapas"), ("madeira", 0.5m, "placas"), ("borracha", 2m, "m")),
            "componente" => BuildFallbackRequirements(("pertinax", 0.8m, "placas")),
            "acessorio" => BuildFallbackRequirements(("madeira", 0.3m, "placas")),
            "servico" => BuildFallbackRequirements(("borracha", 1.5m, "m")),
            "manutencao" => BuildFallbackRequirements(("borracha", 2m, "m")),
            _ => BuildFallbackRequirements(("aco", 0.6m, "chapas"))
        };
    }

    private IEnumerable<ProductMaterialRequirementState> BuildFallbackRequirements(params (string technicalType, decimal quantity, string unit)[] items) =>
        items.Select(item =>
        {
            var material = stateStore.Materials.FirstOrDefault(x => x.TechnicalType == item.technicalType);
            return material is null
                ? null
                : new ProductMaterialRequirementState
                {
                    MaterialId = material.Id,
                    MaterialName = material.Name,
                    QuantityPerUnit = item.quantity,
                    Unit = item.unit
                };
        }).OfType<ProductMaterialRequirementState>();

    private static decimal ResolveEstimatedRevenue(OrderScopeItem item)
    {
        if (item.UnitPrice is > 0)
        {
            return decimal.Round(item.UnitPrice.Value * item.Quantity, 2);
        }

        var fallback = item.Category switch
        {
            "produto_principal" => 1850m,
            "componente" => 720m,
            "acessorio" => 390m,
            "servico" => 520m,
            "manutencao" => 610m,
            _ => 480m
        };

        return fallback * item.Quantity;
    }

    private static string ResolveInitialSector(string category) => category switch
    {
        "servico" => "Montagem",
        "manutencao" => "Emborrachamento",
        "acessorio" => "Montagem",
        _ => "Preparacao"
    };

    private static string ResolveSectorOwner(string sector) => sector switch
    {
        "Preparacao" => "PCP central",
        "Corte" => "Corte e laser",
        "Montagem" => "Montagem",
        "Emborrachamento" => "Setor de borracha",
        "Expedicao" => "Logistica",
        _ => "Operacao"
    };

    private static int ResolveComplexity(int quantity, string category) => Math.Clamp(quantity + (category is "adaptacao" or "manutencao" ? 2 : 1), 1, 5);

    private static string ResolveMaterialSupport(string category, ProductTemplateState? template)
    {
        if (template?.MaterialRequirements.Count > 0)
        {
            return string.Join(" + ", template.MaterialRequirements.Select(x => x.MaterialName));
        }

        return category switch
        {
            "produto_principal" => "aco + madeira + borracha",
            "componente" => "componentes dedicados",
            "acessorio" => "acessorios e apoio",
            "servico" => "insumos de acabamento",
            "manutencao" => "borracha + revisao",
            _ => "combinacao customizada"
        };
    }

    private static string ResolveFinancePaymentMethod(Order order)
    {
        var explicitMethod = order.ScopeItems
            .Select(x => x.BillingMethod)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        return explicitMethod ?? "Boleto";
    }

    private static string ResolvePriority(UrgencyLevel urgency) => urgency switch
    {
        UrgencyLevel.MachineStop => "Parada de maquina",
        UrgencyLevel.Urgent => "Urgente",
        _ => "Normal"
    };

    private static OrderDetailDto Map(Order order, string customerName, AppStateStore stateStore)
    {
        var productionOrders = stateStore.ProductionOrders
            .Where(x => x.OrderId == order.Id)
            .OrderBy(x => x.Number)
            .Select(x => new RelatedProductionOrderDto(
                x.Id,
                x.Number,
                x.Sector,
                x.Status,
                x.Owner,
                x.DueAtUtc))
            .ToList();

        var shipments = stateStore.Shipments
            .Where(x => x.OrderId == order.Id)
            .OrderBy(x => x.ScheduledAtUtc)
            .Select(x => new RelatedShipmentDto(
                x.Id,
                x.ShipmentNumber,
                x.Mode,
                x.Status,
                x.Recipient,
                x.CarrierName,
                x.ScheduledAtUtc))
            .ToList();

        var financeEntries = stateStore.FinanceEntries
            .Where(x => x.OrderId == order.Id)
            .OrderBy(x => x.DueAtUtc)
            .Select(x => new RelatedFinanceEntryDto(
                x.Id,
                x.Type,
                x.Status,
                x.Description,
                x.Amount,
                x.EntrySource,
                x.PaymentMethod,
                x.BoletoStatus,
                x.BoletoNumber,
                x.DueAtUtc))
            .ToList();

        var history = stateStore.AuditLogs
            .Where(x => x.EntityName == nameof(Order) && x.EntityId == order.Id)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new OrderHistoryEntryDto(
                x.Id,
                x.Action,
                x.Description,
                x.ActorName,
                x.OccurredAtUtc))
            .ToList();

        return new OrderDetailDto(
            order.Id,
            order.Number,
            order.CustomerId,
            customerName,
            order.Status.ToString(),
            order.ServiceType.ToString(),
            order.Urgency.ToString(),
            order.ContextSummary,
            order.LegacyAssetReference,
            order.Notes,
            order.ScopeItems
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new OrderScopeItemDto(
                    x.Id,
                    x.Title,
                    x.Category,
                    x.Quantity,
                    x.ProductTemplateId,
                    x.ProductName,
                    x.BillingMethod,
                    x.UnitPrice,
                    x.Notes))
                .ToList(),
            order.Attachments
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new OrderAttachmentDto(
                    x.Id,
                    x.OriginalFileName,
                    x.ContentType,
                    x.SizeBytes,
                    x.Sha256,
                    x.CreatedAtUtc))
                .ToList(),
            order.Analyses
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new TechnicalAnalysisDto(
                    x.Id,
                    x.AttachmentId,
                    x.SourceFileExtension,
                    x.Status.ToString(),
                    x.Summary,
                    x.EngineName,
                    x.ConfidencePercent,
                    x.CreatedAtUtc))
                .ToList(),
            productionOrders,
            shipments,
            financeEntries,
            history,
            order.CreatedAtUtc);
    }
}
