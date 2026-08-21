using PackControl.Application.Abstractions;
using System.Text.Json;
using PackControl.Domain.Audit;
using PackControl.Domain.Customers;
using PackControl.Domain.Identity;
using PackControl.Domain.Orders;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class DevelopmentDataSeeder(
    AppStateStore stateStore,
    IClock clock,
    PasswordService passwordService,
    IAppStatePersistence statePersistence)
{
    public async Task EnsureSeededAsync(CancellationToken cancellationToken)
    {
        lock (stateStore.SyncRoot)
        {
            if (!stateStore.Users.Any())
            {
                var admin = AppUser.Create(
                    "admin@packcontrol.local",
                    "Administrador PackControl",
                    UserRole.Administrator,
                    string.Empty,
                    clock.UtcNow,
                    "bootstrap");

                admin.SetPasswordHash(passwordService.Hash("PackControl!123"), clock.UtcNow, "bootstrap");
                stateStore.Users.Add(admin);

                var commercial = AppUser.Create(
                    "comercial@packcontrol.local",
                    "Ana do Comercial",
                    UserRole.Sales,
                    string.Empty,
                    clock.UtcNow,
                    "bootstrap");
                commercial.SetPasswordHash(passwordService.Hash("PackControl!123"), clock.UtcNow, "bootstrap");
                stateStore.Users.Add(commercial);

                var engineer = AppUser.Create(
                    "engenharia@packcontrol.local",
                    "Bruno da Engenharia",
                    UserRole.Engineering,
                    string.Empty,
                    clock.UtcNow,
                    "bootstrap");
                engineer.SetPasswordHash(passwordService.Hash("PackControl!123"), clock.UtcNow, "bootstrap");
                stateStore.Users.Add(engineer);

                var finance = AppUser.Create(
                    "financeiro@packcontrol.local",
                    "Carla do Financeiro",
                    UserRole.Finance,
                    string.Empty,
                    clock.UtcNow,
                    "bootstrap");
                finance.SetPasswordHash(passwordService.Hash("PackControl!123"), clock.UtcNow, "bootstrap");
                stateStore.Users.Add(finance);
            }

            if (!stateStore.Materials.Any())
            {
                stateStore.Materials.Add(new MaterialCatalogItemState
                {
                    Id = Guid.NewGuid(),
                    Name = "Aco 2mm temperado",
                    TechnicalType = "aco",
                    Category = "estrutura",
                    MainSupplier = "Acos Brasil",
                    RiskLevel = "Medio",
                    StandardCost = 184.50m,
                    LeadTimeDays = 4,
                    Unit = "chapas"
                });

                stateStore.Materials.Add(new MaterialCatalogItemState
                {
                    Id = Guid.NewGuid(),
                    Name = "Borracha vermelha 60 shore",
                    TechnicalType = "borracha",
                    Category = "acabamento",
                    MainSupplier = "Borrachas Sul",
                    RiskLevel = "Baixo",
                    StandardCost = 42.90m,
                    LeadTimeDays = 2,
                    Unit = "m"
                });

                stateStore.Materials.Add(new MaterialCatalogItemState
                {
                    Id = Guid.NewGuid(),
                    Name = "Madeira naval 18mm",
                    TechnicalType = "madeira",
                    Category = "base",
                    MainSupplier = "Madeireira Norte",
                    RiskLevel = "Critico",
                    StandardCost = 96.00m,
                    LeadTimeDays = 6,
                    Unit = "placas"
                });

                stateStore.Materials.Add(new MaterialCatalogItemState
                {
                    Id = Guid.NewGuid(),
                    Name = "Pertinax estrutural",
                    TechnicalType = "pertinax",
                    Category = "componente",
                    MainSupplier = "CompTech",
                    RiskLevel = "Medio",
                    StandardCost = 67.40m,
                    LeadTimeDays = 3,
                    Unit = "placas"
                });
            }

            if (!stateStore.Carriers.Any())
            {
                stateStore.Carriers.Add(new CarrierState
                {
                    Id = Guid.NewGuid(),
                    Name = "Expresso Facaria Sul",
                    ContactName = "Marcos Ribeiro",
                    Email = "operacao@expressosul.local",
                    Phone = "(11) 3222-9000",
                    BusinessHours = "Seg a sex, 07h as 18h",
                    ServiceArea = "Grande Sao Paulo e interior",
                    DefaultMode = "Entrega terceirizada",
                    DoesPickup = true,
                    DoesDelivery = true,
                    Notes = "Coleta com agenda minima de 4 horas.",
                    UpdatedAtUtc = clock.UtcNow
                });

                stateStore.Carriers.Add(new CarrierState
                {
                    Id = Guid.NewGuid(),
                    Name = "Retira Bem Log",
                    ContactName = "Juliana Prado",
                    Email = "coleta@retirabem.local",
                    Phone = "(11) 3555-1122",
                    BusinessHours = "Seg a sab, 06h as 20h",
                    ServiceArea = "Capital e ABC",
                    DefaultMode = "Retirada",
                    DoesPickup = true,
                    DoesDelivery = false,
                    Notes = "Foco em retirada de producao pronta.",
                    UpdatedAtUtc = clock.UtcNow
                });
            }

            if (!stateStore.Customers.Any())
            {
                var defaultCarrier = stateStore.Carriers.First();
                stateStore.Customers.Add(Customer.Create(
                    "Industria Grafica Pacheco",
                    "12.345.678/0001-90",
                    "Felix Pacheco",
                    "felix@pacheco.local",
                    "(11) 4000-1000",
                    "Cliente inicial para bootstrap do MVP.",
                    ["Pacheco", "Grafica Pacheco"],
                    "01311-000",
                    "Avenida Paulista",
                    "1450",
                    "Bela Vista",
                    "Sao Paulo",
                    "SP",
                    "3550308",
                    "110.042.490.114",
                    "Contribuinte",
                    "Conjunto 21",
                    "Proximo ao MASP",
                    defaultCarrier.Id,
                    defaultCarrier.Name,
                    "Entrega terceirizada",
                    [],
                    78,
                    clock.UtcNow,
                    "bootstrap"));

                stateStore.Customers.Add(Customer.Create(
                    "Metalurgica Central",
                    "98.765.432/0001-10",
                    "Larissa Moraes",
                    "larissa@metalurgica.local",
                    "(11) 4111-2020",
                    "Usado para validar fluxo de repeticao e adaptacao.",
                    ["Central", "Metal Central"],
                    "09540-500",
                    "Rua Amazonas",
                    "780",
                    "Centro",
                    "Sao Caetano do Sul",
                    "SP",
                    "3548807",
                    "636.903.722.119",
                    "Contribuinte",
                    null,
                    "Fundos do galpao 4",
                    null,
                    null,
                    "Retirada",
                    [],
                    84,
                    clock.UtcNow,
                    "bootstrap"));
            }

            if (!stateStore.StockItems.Any())
            {
                foreach (var material in stateStore.Materials)
                {
                    stateStore.StockItems.Add(new StockItemState
                    {
                        Id = Guid.NewGuid(),
                        MaterialId = material.Id,
                        MaterialName = material.Name,
                        OnHand = material.Name switch
                        {
                            "Madeira naval 18mm" => 8,
                            "Borracha vermelha 60 shore" => 34,
                            "Pertinax estrutural" => 14,
                            _ => 18
                        },
                        Reserved = material.Name switch
                        {
                            "Madeira naval 18mm" => 5,
                            _ => 2
                        },
                        ReorderPoint = material.Name switch
                        {
                            "Borracha vermelha 60 shore" => 10,
                            "Madeira naval 18mm" => 7,
                            _ => 6
                        },
                        LastMovement = "Carga inicial da baseline",
                        LastMovementAtUtc = clock.UtcNow.AddDays(-1)
                    });
                }
            }

            if (!stateStore.ProductTemplates.Any())
            {
                var aco = stateStore.Materials.Single(x => x.TechnicalType == "aco");
                var borracha = stateStore.Materials.Single(x => x.TechnicalType == "borracha");
                var madeira = stateStore.Materials.Single(x => x.TechnicalType == "madeira");
                var pertinax = stateStore.Materials.Single(x => x.TechnicalType == "pertinax");

                stateStore.ProductTemplates.Add(new ProductTemplateState
                {
                    Id = Guid.NewGuid(),
                    Name = "Faca caixa",
                    Category = "produto_principal",
                    Description = "Conjunto padrao para facas de caixa com quadro e borracha.",
                    BillingMethod = "Por unidade",
                    DefaultUnitPrice = 2480m,
                    DefaultProductionSector = "Preparacao",
                    FiscalNcm = "8208.90.00",
                    FiscalCfop = "5101",
                    FiscalCommercialUnit = "UN",
                    FiscalOriginCode = "0",
                    FiscalIcmsSituationCode = "00",
                    FiscalIpiSituationCode = "99",
                    FiscalPisSituationCode = "49",
                    FiscalCofinsSituationCode = "49",
                    FiscalIcmsRate = 18m,
                    FiscalIpiRate = 0m,
                    FiscalPisRate = 1.65m,
                    FiscalCofinsRate = 7.6m,
                    Active = true,
                    MaterialRequirements =
                    [
                        new ProductMaterialRequirementState
                        {
                            MaterialId = aco.Id,
                            MaterialName = aco.Name,
                            QuantityPerUnit = 1m,
                            Unit = "chapas"
                        },
                        new ProductMaterialRequirementState
                        {
                            MaterialId = madeira.Id,
                            MaterialName = madeira.Name,
                            QuantityPerUnit = 0.5m,
                            Unit = "placas"
                        },
                        new ProductMaterialRequirementState
                        {
                            MaterialId = borracha.Id,
                            MaterialName = borracha.Name,
                            QuantityPerUnit = 2m,
                            Unit = "m"
                        }
                    ],
                    UpdatedAtUtc = clock.UtcNow
                });

                stateStore.ProductTemplates.Add(new ProductTemplateState
                {
                    Id = Guid.NewGuid(),
                    Name = "Faca adesivo",
                    Category = "produto_principal",
                    Description = "Produto comercial focado em adesivos e tiragens curtas.",
                    BillingMethod = "Por milheiro",
                    DefaultUnitPrice = 1780m,
                    DefaultProductionSector = "Montagem",
                    FiscalNcm = "8208.90.00",
                    FiscalCfop = "5102",
                    FiscalCommercialUnit = "MIL",
                    FiscalOriginCode = "0",
                    FiscalIcmsSituationCode = "00",
                    FiscalIpiSituationCode = "99",
                    FiscalPisSituationCode = "49",
                    FiscalCofinsSituationCode = "49",
                    FiscalIcmsRate = 18m,
                    FiscalIpiRate = 0m,
                    FiscalPisRate = 1.65m,
                    FiscalCofinsRate = 7.6m,
                    Active = true,
                    MaterialRequirements =
                    [
                        new ProductMaterialRequirementState
                        {
                            MaterialId = aco.Id,
                            MaterialName = aco.Name,
                            QuantityPerUnit = 0.7m,
                            Unit = "chapas"
                        },
                        new ProductMaterialRequirementState
                        {
                            MaterialId = borracha.Id,
                            MaterialName = borracha.Name,
                            QuantityPerUnit = 1.2m,
                            Unit = "m"
                        }
                    ],
                    UpdatedAtUtc = clock.UtcNow
                });

                stateStore.ProductTemplates.Add(new ProductTemplateState
                {
                    Id = Guid.NewGuid(),
                    Name = "Montagem tecnica",
                    Category = "servico",
                    Description = "Servico comercial para montagem final e regulagem.",
                    BillingMethod = "Por hora tecnica",
                    DefaultUnitPrice = 420m,
                    DefaultProductionSector = "Montagem",
                    FiscalNcm = "9985.19.90",
                    FiscalCfop = "5933",
                    FiscalCommercialUnit = "HR",
                    FiscalOriginCode = "0",
                    FiscalIcmsSituationCode = "41",
                    FiscalIpiSituationCode = "99",
                    FiscalPisSituationCode = "49",
                    FiscalCofinsSituationCode = "49",
                    FiscalIcmsRate = 0m,
                    FiscalIpiRate = 0m,
                    FiscalPisRate = 0.65m,
                    FiscalCofinsRate = 3m,
                    Active = true,
                    MaterialRequirements =
                    [
                        new ProductMaterialRequirementState
                        {
                            MaterialId = pertinax.Id,
                            MaterialName = pertinax.Name,
                            QuantityPerUnit = 0.3m,
                            Unit = "placas"
                        }
                    ],
                    UpdatedAtUtc = clock.UtcNow
                });

                stateStore.ProductTemplates.Add(new ProductTemplateState
                {
                    Id = Guid.NewGuid(),
                    Name = "Emborrachamento padrao",
                    Category = "manutencao",
                    Description = "Servico de aplicacao e troca de borracha.",
                    BillingMethod = "Por metro aplicado",
                    DefaultUnitPrice = 185m,
                    DefaultProductionSector = "Emborrachamento",
                    FiscalNcm = "4016.99.90",
                    FiscalCfop = "5102",
                    FiscalCommercialUnit = "M",
                    FiscalOriginCode = "0",
                    FiscalIcmsSituationCode = "00",
                    FiscalIpiSituationCode = "99",
                    FiscalPisSituationCode = "49",
                    FiscalCofinsSituationCode = "49",
                    FiscalIcmsRate = 18m,
                    FiscalIpiRate = 0m,
                    FiscalPisRate = 1.65m,
                    FiscalCofinsRate = 7.6m,
                    Active = true,
                    MaterialRequirements =
                    [
                        new ProductMaterialRequirementState
                        {
                            MaterialId = borracha.Id,
                            MaterialName = borracha.Name,
                            QuantityPerUnit = 2.4m,
                            Unit = "m"
                        }
                    ],
                    UpdatedAtUtc = clock.UtcNow
                });
            }

            if (stateStore.Customers.Any() && stateStore.ProductTemplates.Any() && stateStore.Customers.All(x => x.ProductPricingRules.Count == 0))
            {
                var facaCaixa = stateStore.ProductTemplates.Single(x => x.Name == "Faca caixa");
                var emborrachamentoPadrao = stateStore.ProductTemplates.Single(x => x.Name == "Emborrachamento padrao");
                var firstCustomer = stateStore.Customers.OrderBy(x => x.Name).First();
                var secondCustomer = stateStore.Customers.OrderBy(x => x.Name).Skip(1).First();

                firstCustomer.Update(
                    firstCustomer.Name,
                    firstCustomer.DocumentNumber,
                    firstCustomer.ContactName,
                    firstCustomer.Email,
                    firstCustomer.Phone,
                    firstCustomer.Notes,
                    firstCustomer.Nicknames,
                    firstCustomer.PostalCode,
                    firstCustomer.Street,
                    firstCustomer.StreetNumber,
                    firstCustomer.District,
                    firstCustomer.City,
                    firstCustomer.State,
                    firstCustomer.CityIbgeCode,
                    firstCustomer.StateRegistration,
                    firstCustomer.TaxpayerIndicator,
                    firstCustomer.Complement,
                    firstCustomer.ReferencePoint,
                    firstCustomer.DefaultCarrierId,
                    firstCustomer.DefaultCarrierName,
                    firstCustomer.DefaultDeliveryMode,
                    [
                        CustomerProductPricingRule.Create(facaCaixa.Id, facaCaixa.Name, "Por unidade", 2320m, "Tabela cliente estrategico."),
                        CustomerProductPricingRule.Create(emborrachamentoPadrao.Id, emborrachamentoPadrao.Name, "Por metro aplicado", 168m, "Contrato de manutencao.")
                    ],
                    firstCustomer.Score,
                    clock.UtcNow,
                    "bootstrap");

                secondCustomer.Update(
                    secondCustomer.Name,
                    secondCustomer.DocumentNumber,
                    secondCustomer.ContactName,
                    secondCustomer.Email,
                    secondCustomer.Phone,
                    secondCustomer.Notes,
                    secondCustomer.Nicknames,
                    secondCustomer.PostalCode,
                    secondCustomer.Street,
                    secondCustomer.StreetNumber,
                    secondCustomer.District,
                    secondCustomer.City,
                    secondCustomer.State,
                    secondCustomer.CityIbgeCode,
                    secondCustomer.StateRegistration,
                    secondCustomer.TaxpayerIndicator,
                    secondCustomer.Complement,
                    secondCustomer.ReferencePoint,
                    secondCustomer.DefaultCarrierId,
                    secondCustomer.DefaultCarrierName,
                    secondCustomer.DefaultDeliveryMode,
                    [
                        CustomerProductPricingRule.Create(facaCaixa.Id, facaCaixa.Name, "Por unidade", 2410m, "Faixa comercial de repeticao.")
                    ],
                    secondCustomer.Score,
                    clock.UtcNow,
                    "bootstrap");
            }

            if (!stateStore.RegisterEntries.Any())
            {
                stateStore.RegisterEntries.AddRange(
                [
                    CreateRegisterEntry("tipos_faca", "Tipos de faca", "Faca plana", "Padrao principal de faca de corte."),
                    CreateRegisterEntry("tipos_faca", "Tipos de faca", "Faca rotativa", "Usada em operacoes continuas."),
                    CreateRegisterEntry("tipos_destacador", "Tipos de destacador", "Dinamico", "Exige ajuste operacional no conjunto."),
                    CreateRegisterEntry("tipos_destacador", "Tipos de destacador", "Convencional", "Configuracao tradicional de destacador."),
                    CreateRegisterEntry("tipos_borracha", "Tipos de borracha", "Vermelha 60 shore", "Borracha padrao de acabamento."),
                    CreateRegisterEntry("tipos_borracha", "Tipos de borracha", "Cinza antidesgaste", "Aplicada em itens de alto ciclo."),
                    CreateRegisterEntry("tipos_material", "Tipos de material", "Aco", "Grupo tecnico para chapas e laminas."),
                    CreateRegisterEntry("tipos_material", "Tipos de material", "Madeira", "Grupo base para quadros e suportes."),
                    CreateRegisterEntry("tipos_material", "Tipos de material", "Pertinax", "Grupo tecnico para componentes estruturais."),
                    CreateRegisterEntry("setores", "Setores", "Preparacao", "PCP e pre-separacao de trabalho."),
                    CreateRegisterEntry("setores", "Setores", "Corte", "Corte, laser e dobra."),
                    CreateRegisterEntry("setores", "Setores", "Montagem", "Montagem e ajustes de conjunto."),
                    CreateRegisterEntry("setores", "Setores", "Emborrachamento", "Aplicacao e revisao de borracha."),
                    CreateRegisterEntry("setores", "Setores", "Expedicao", "Conferencia e liberacao logistica."),
                    CreateRegisterEntry("operacoes", "Operacoes", "Corte laser", "Corte principal de aco e derivados."),
                    CreateRegisterEntry("operacoes", "Operacoes", "Montagem manual", "Montagem final dos componentes."),
                    CreateRegisterEntry("operacoes", "Operacoes", "Emborrachamento", "Aplicacao de borracha por metragem."),
                    CreateRegisterEntry("modos_entrega", "Modos de entrega", "Entrega propria", "Motorista e veiculo da empresa."),
                    CreateRegisterEntry("modos_entrega", "Modos de entrega", "Retirada", "Cliente retira no local."),
                    CreateRegisterEntry("modos_entrega", "Modos de entrega", "Terceiro", "Coleta ou entrega por parceiro."),
                    CreateRegisterEntry("fornecedores", "Fornecedores", "Acos Brasil", "Fornecedor principal de aco temperado."),
                    CreateRegisterEntry("fornecedores", "Fornecedores", "Borrachas Sul", "Fornecedor principal de borracha."),
                    CreateRegisterEntry("fornecedores", "Fornecedores", "Madeireira Norte", "Fornecedor de bases e placas."),
                    CreateRegisterEntry("unidades_medida", "Unidades de medida", "un", "Unidade."),
                    CreateRegisterEntry("unidades_medida", "Unidades de medida", "m", "Metro."),
                    CreateRegisterEntry("unidades_medida", "Unidades de medida", "placa", "Placa ou chapa inteira.")
                ]);
            }

            if (!stateStore.Orders.Any())
            {
                var firstCustomer = stateStore.Customers.OrderBy(x => x.Name).First();
                var secondCustomer = stateStore.Customers.OrderBy(x => x.Name).Skip(1).First();

                var draftOrder = Order.Create(
                    "PED-BOOT-001",
                    firstCustomer.Id,
                    ServiceType.New,
                    UrgencyLevel.Normal,
                    "Cliente iniciou atendimento sem arquivo e com definicao parcial do conjunto.",
                    null,
                    "Pedido seed para demonstracao do fluxo basico.",
                    clock.UtcNow,
                    "bootstrap");

                var facaCaixa = stateStore.ProductTemplates.Single(x => x.Name == "Faca caixa");
                var emborrachamento = stateStore.ProductTemplates.Single(x => x.Name == "Emborrachamento padrao");

                draftOrder.AddScopeItem(
                    "Faca principal",
                    "produto_principal",
                    1,
                    facaCaixa.Id,
                    facaCaixa.Name,
                    facaCaixa.BillingMethod,
                    facaCaixa.DefaultUnitPrice,
                    "Componente principal da faca.",
                    clock.UtcNow,
                    "bootstrap");
                draftOrder.AddScopeItem(
                    "Emborrachamento",
                    "servico",
                    1,
                    emborrachamento.Id,
                    emborrachamento.Name,
                    emborrachamento.BillingMethod,
                    emborrachamento.DefaultUnitPrice,
                    "Servico agregado na primeira entrega.",
                    clock.UtcNow,
                    "bootstrap");

                var approvedOrder = Order.Create(
                    "PED-BOOT-002",
                    secondCustomer.Id,
                    ServiceType.Repeat,
                    UrgencyLevel.Urgent,
                    "Repeticao parcial com troca de componentes e reaproveitamento de referencia antiga.",
                    "ATV-2198 / FACA CLIENTE",
                    "Pedido seed com operacao projetada entre producao, logistica e financeiro.",
                    clock.UtcNow.AddHours(-12),
                    "bootstrap");

                var facaAdesivo = stateStore.ProductTemplates.Single(x => x.Name == "Faca adesivo");
                var montagemTecnica = stateStore.ProductTemplates.Single(x => x.Name == "Montagem tecnica");

                approvedOrder.AddScopeItem(
                    "Quadro principal",
                    "produto_principal",
                    1,
                    facaAdesivo.Id,
                    facaAdesivo.Name,
                    facaAdesivo.BillingMethod,
                    facaAdesivo.DefaultUnitPrice,
                    "Estrutura principal reaproveitando referencia antiga.",
                    clock.UtcNow.AddHours(-12),
                    "bootstrap");
                approvedOrder.AddScopeItem(
                    "Jogo de laminas",
                    "componente",
                    2,
                    montagemTecnica.Id,
                    montagemTecnica.Name,
                    montagemTecnica.BillingMethod,
                    520m,
                    "Substituicao por desgaste.",
                    clock.UtcNow.AddHours(-12),
                    "bootstrap");
                approvedOrder.Approve(clock.UtcNow.AddHours(-10), "bootstrap");
                approvedOrder.MarkInProduction(clock.UtcNow.AddHours(-8), "bootstrap");

                stateStore.Orders.Add(draftOrder);
                stateStore.Orders.Add(approvedOrder);
                stateStore.AuditLogs.Add(AuditLog.Create(
                    null,
                    "bootstrap",
                    nameof(Order),
                    draftOrder.Id,
                    "order.seeded",
                    "Pedido inicial criado para demonstracao do fluxo comercial.",
                    null,
                    clock.UtcNow));

                stateStore.AuditLogs.Add(AuditLog.Create(
                    null,
                    "bootstrap",
                    nameof(Order),
                    approvedOrder.Id,
                    "order.seeded",
                    "Pedido aprovado seedado para demonstracao do fluxo operacional.",
                    null,
                    clock.UtcNow.AddHours(-10)));
            }

            var orderForOperations = stateStore.Orders.SingleOrDefault(x => x.Number == "PED-BOOT-002");
            if (orderForOperations is not null && !stateStore.ProductionOrders.Any(x => x.OrderId == orderForOperations.Id))
            {
                var customerName = stateStore.Customers.Single(x => x.Id == orderForOperations.CustomerId).Name;
                stateStore.ProductionOrders.Add(new ProductionOrderState
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderForOperations.Id,
                    OrderNumber = orderForOperations.Number,
                    Number = "OP-PED-BOOT-002-01",
                    CustomerName = customerName,
                    Title = "Quadro principal",
                    Quantity = 1,
                    ProductTemplateId = stateStore.ProductTemplates.Single(x => x.Name == "Faca adesivo").Id,
                    ProductName = "Faca adesivo",
                    BillingMethod = "Por milheiro",
                    UnitPrice = 1780m,
                    Sector = "Corte",
                    Status = "Em producao",
                    Priority = "Urgente",
                    Owner = "Corte e laser",
                    Complexity = 4,
                    Outsourced = false,
                    MaterialSupport = "aco + madeira",
                    DueAtUtc = clock.UtcNow.AddDays(1),
                    UpdatedAtUtc = clock.UtcNow.AddHours(-2)
                });

                stateStore.ProductionOrders.Add(new ProductionOrderState
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderForOperations.Id,
                    OrderNumber = orderForOperations.Number,
                    Number = "OP-PED-BOOT-002-02",
                    CustomerName = customerName,
                    Title = "Jogo de laminas",
                    Quantity = 2,
                    ProductTemplateId = stateStore.ProductTemplates.Single(x => x.Name == "Montagem tecnica").Id,
                    ProductName = "Montagem tecnica",
                    BillingMethod = "Por hora tecnica",
                    UnitPrice = 520m,
                    Sector = "Montagem",
                    Status = "Aguardando fila",
                    Priority = "Urgente",
                    Owner = "Montagem",
                    Complexity = 3,
                    Outsourced = false,
                    MaterialSupport = "componentes dedicados",
                    DueAtUtc = clock.UtcNow.AddDays(2),
                    UpdatedAtUtc = clock.UtcNow.AddHours(-1)
                });
            }

            if (orderForOperations is not null && !stateStore.Shipments.Any(x => x.OrderId == orderForOperations.Id))
            {
                var customerName = stateStore.Customers.Single(x => x.Id == orderForOperations.CustomerId).Name;
                stateStore.Shipments.Add(new ShipmentState
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderForOperations.Id,
                    OrderNumber = orderForOperations.Number,
                    ShipmentNumber = "LOT-PED-BOOT-002",
                    CustomerName = customerName,
                    Mode = "Entrega terceirizada",
                    Status = "Aguardando producao",
                    Recipient = customerName,
                    CarrierId = stateStore.Carriers.First().Id,
                    CarrierName = stateStore.Carriers.First().Name,
                    DriverName = stateStore.Carriers.First().ContactName,
                    VehiclePlate = "TER-1024",
                    ChecklistStatus = "Pendente",
                    HasOccurrence = false,
                    ScheduledAtUtc = clock.UtcNow.AddDays(3)
                });
            }

            if (orderForOperations is not null && !stateStore.FinanceEntries.Any(x => x.OrderId == orderForOperations.Id))
            {
                var customerName = stateStore.Customers.Single(x => x.Id == orderForOperations.CustomerId).Name;
                stateStore.FinanceEntries.Add(new FinanceEntryState
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderForOperations.Id,
                    OrderNumber = orderForOperations.Number,
                    Type = "Receber",
                    Status = "Em aberto",
                    Description = $"Faturamento previsto do pedido {orderForOperations.Number}",
                    Counterparty = customerName,
                    Amount = 4680m,
                    DueAtUtc = clock.UtcNow.AddDays(12),
                    EntrySource = "Pedido",
                    PaymentMethod = "Boleto",
                    Notes = "Titulo seedado para demonstracao."
                });

                stateStore.FinanceEntries.Add(new FinanceEntryState
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderForOperations.Id,
                    OrderNumber = orderForOperations.Number,
                    Type = "Pagar",
                    Status = "Programado",
                    Description = $"Compra prevista de insumos para {orderForOperations.Number}",
                    Counterparty = "Fornecedor principal",
                    Amount = 1580m,
                    DueAtUtc = clock.UtcNow.AddDays(6),
                    EntrySource = "Pedido",
                    PaymentMethod = "Transferencia",
                    Notes = "Programacao de compra seedada."
                });
            }

            if (!stateStore.FiscalCompanies.Any())
            {
                stateStore.FiscalCompanies.Add(new FiscalCompanyProfileState
                {
                    Id = Guid.NewGuid(),
                    TradeName = "PackControl Facaria Ltda",
                    DocumentNumber = "12.345.678/0001-90",
                    StateRegistration = "123.456.789.000",
                    TaxRegime = "Lucro Presumido",
                    PostalCode = "01311-000",
                    Street = "Avenida Paulista",
                    StreetNumber = "1450",
                    District = "Bela Vista",
                    City = "Sao Paulo",
                    StateCode = "SP",
                    CityIbgeCode = "3550308",
                    Country = "Brasil",
                    Complement = "Conjunto 2101",
                    FiscalSeries = "1",
                    NfeEnabled = true,
                    Environment = "Homologacao",
                    AdapterName = "mock-plugavel",
                    CertificateType = "A1/A3",
                    CertificateMedia = "Arquivo, pendrive ou cartao",
                    PrincipalEmissionMode = "A1",
                    ContingencyEmissionMode = "A3",
                    CertificateLabel = "Repositorio fiscal principal",
                    CertificateSerialNumber = "SERIAL-DEMO-0001",
                    AccountantValidated = true,
                    HomologationCredentialsValidated = true,
                    HomologationApproved = true,
                    ProductionCredentialsValidated = false,
                    ProductionApproved = false,
                    OnboardingNotes = "Emitente seedado em homologacao para smoke fiscal.",
                    LastNfeNumber = 120
                });
            }

            if (!stateStore.FiscalOperationTemplates.Any())
            {
                var companyId = stateStore.FiscalCompanies.First().Id;
                stateStore.FiscalOperationTemplates.Add(new FiscalOperationTemplateState
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = companyId,
                    Name = "Venda padrao de facaria",
                    NatureOfOperation = "Venda de produto",
                    Cfop = "5101",
                    Finality = "Normal",
                    Active = true,
                    Notes = "Template fiscal inicial para o modulo plugavel.",
                    UpdatedAtUtc = clock.UtcNow
                });
            }

            if (!stateStore.FiscalAgents.Any())
            {
                stateStore.FiscalAgents.Add(new FiscalAgentRegistrationState
                {
                    Id = Guid.NewGuid(),
                    Name = "Fiscal Agent demo",
                    Hostname = "demo-a3.local",
                    CertificateMedia = "Arquivo, pendrive ou cartao",
                    Online = false,
                    LastSeenAtUtc = clock.UtcNow.AddMinutes(-45),
                    Status = "Planejado",
                    Notes = "Registro inicial do agente local para fluxos A3."
                });
            }

            if (!stateStore.FiscalInvoices.Any())
            {
                var receiveEntry = stateStore.FinanceEntries.FirstOrDefault(x => x.Type == "Receber");
                if (receiveEntry is not null)
                {
                    stateStore.FiscalInvoices.Add(new FiscalInvoiceState
                    {
                        Id = Guid.NewGuid(),
                        FinanceEntryId = receiveEntry.Id,
                        OrderId = receiveEntry.OrderId,
                        OrderNumber = receiveEntry.OrderNumber,
                        Number = "000000120",
                        Series = "1",
                        Environment = "Homologacao",
                        AccessKey = "1234567890123456789012",
                        Protocol = "135202603250001",
                        EngineName = "mock-plugavel",
                        CertificateType = "A1/A3",
                        CertificateMedia = "Arquivo, pendrive ou cartao",
                        NatureOfOperation = "Venda de produto",
                        Cfop = "5101",
                        XmlArchivePath = null,
                        DanfeArchivePath = null,
                        CustomerName = receiveEntry.Counterparty,
                        Status = "Emitida para adaptador fiscal",
                        Amount = receiveEntry.Amount,
                        IssuedAtUtc = clock.UtcNow.AddDays(-1),
                        Notes = "NF-e seedada para demonstracao do adaptador fiscal."
                    });
                }
            }

            if (!stateStore.FiscalDocuments.Any() && stateStore.FiscalInvoices.Any())
            {
                var company = stateStore.FiscalCompanies.First();
                foreach (var invoice in stateStore.FiscalInvoices)
                {
                    stateStore.FiscalDocuments.Add(new FiscalDocumentState
                    {
                        Id = invoice.Id,
                        CompanyProfileId = company.Id,
                        FinanceEntryId = invoice.FinanceEntryId,
                        OrderId = invoice.OrderId,
                        OrderNumber = invoice.OrderNumber,
                        Number = invoice.Number,
                        Series = invoice.Series,
                        Environment = invoice.Environment,
                        AccessKey = invoice.AccessKey,
                        Protocol = invoice.Protocol,
                        AdapterName = invoice.EngineName,
                        IssueMode = "Hibrido A1/A3",
                        CertificateType = invoice.CertificateType,
                        CertificateMedia = invoice.CertificateMedia,
                        NatureOfOperation = invoice.NatureOfOperation,
                        Cfop = invoice.Cfop,
                        RecipientName = invoice.CustomerName,
                        RecipientDocument = null,
                        Amount = invoice.Amount,
                        Status = "Authorized",
                        LastError = null,
                        AttemptsCount = 1,
                        XmlArchivePath = invoice.XmlArchivePath,
                        DanfeArchivePath = invoice.DanfeArchivePath,
                        CreatedAtUtc = invoice.IssuedAtUtc,
                        IssuedAtUtc = invoice.IssuedAtUtc,
                        UpdatedAtUtc = invoice.IssuedAtUtc,
                        Notes = invoice.Notes
                    });

                    stateStore.FiscalTransmissionAttempts.Add(new FiscalTransmissionAttemptState
                    {
                        Id = Guid.NewGuid(),
                        FiscalDocumentId = invoice.Id,
                        AttemptNumber = 1,
                        Operation = "issue",
                        AdapterName = invoice.EngineName,
                        Status = "Succeeded",
                        ResponseCode = invoice.Protocol,
                        ResponseSummary = invoice.Status,
                        AttemptedAtUtc = invoice.IssuedAtUtc
                    });

                    stateStore.FiscalEvents.Add(new FiscalEventState
                    {
                        Id = Guid.NewGuid(),
                        FiscalDocumentId = invoice.Id,
                        EventType = "authorized",
                        Description = $"NF-e {invoice.Number} seedada como autorizada.",
                        PayloadJson = JsonSerializer.Serialize(new
                        {
                            invoice.AccessKey,
                            invoice.Protocol,
                            invoice.Amount
                        }),
                        ActorUserId = null,
                        ActorName = "System Seeder",
                        OccurredAtUtc = invoice.IssuedAtUtc
                    });
                }
            }

            if (!stateStore.TechnicalAssets.Any())
            {
                var firstCustomer = stateStore.Customers.OrderBy(x => x.Name).First();
                var secondCustomer = stateStore.Customers.OrderBy(x => x.Name).Skip(1).First();

                stateStore.TechnicalAssets.Add(new TechnicalAssetState
                {
                    Id = Guid.NewGuid(),
                    CustomerId = firstCustomer.Id,
                    CustomerName = firstCustomer.Name,
                    Code = "ATV-FC-001",
                    Alias = "Faca caixa Pacheco",
                    AssetType = "Faca completa",
                    Status = "Ativa",
                    Revision = "R3",
                    Components = ["Quadro", "Aco", "Borracha"],
                    Materials = ["Aco 2mm temperado", "Borracha vermelha 60 shore"],
                    LastOrderNumber = "PED-BOOT-001",
                    Notes = "Base usada em repeticoes e manutencao parcial.",
                    UpdatedAtUtc = clock.UtcNow
                });

                stateStore.TechnicalAssets.Add(new TechnicalAssetState
                {
                    Id = Guid.NewGuid(),
                    CustomerId = secondCustomer.Id,
                    CustomerName = secondCustomer.Name,
                    Code = "ATV-MC-014",
                    Alias = "Conjunto metalurgica",
                    AssetType = "Conjunto",
                    Status = "Em revisao",
                    Revision = "R8",
                    Components = ["Quadro", "Laminas", "Destacador"],
                    Materials = ["Aco 2mm temperado", "Pertinax estrutural"],
                    LastOrderNumber = "PED-BOOT-002",
                    Notes = "Historico de adaptacoes e trocas de laminas.",
                    UpdatedAtUtc = clock.UtcNow
                });
            }
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);

        RegisterEntryState CreateRegisterEntry(string groupKey, string groupLabel, string name, string description) => new()
        {
            Id = Guid.NewGuid(),
            GroupKey = groupKey,
            GroupLabel = groupLabel,
            Name = name,
            Description = description,
            Active = true,
            UpdatedAtUtc = clock.UtcNow
        };
    }
}
