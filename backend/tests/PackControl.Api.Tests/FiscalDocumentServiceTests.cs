using PackControl.Application.Abstractions;
using PackControl.Application.Fiscal;
using PackControl.Domain.Identity;
using PackControl.Infrastructure.Persistence;
using PackControl.Infrastructure.Services;

namespace PackControl.Api.Tests;

public sealed class FiscalDocumentServiceTests
{
    [Fact]
    public async Task CancelAsync_ShouldMarkDocumentAsCancelled_AndUpdateLegacyInvoice()
    {
        var stateStore = new AppStateStore();
        var company = BuildCompany();
        var document = BuildAuthorizedDocument(company.Id);
        stateStore.FiscalCompanies.Add(company);
        stateStore.FiscalDocuments.Add(document);
        stateStore.FiscalInvoices.Add(new FiscalInvoiceState
        {
            Id = document.Id,
            Number = document.Number,
            Series = document.Series,
            Status = "Emitida"
        });

        var service = CreateService(
            stateStore,
            new FakeFiscalEngine
            {
                CancelHandler = _ => Task.FromResult(new FiscalNfeEventResult(
                    "Cancelamento registrado",
                    "13520260328120001",
                    "mock-plugavel",
                    "<evento>cancelamento</evento>",
                    "<html>cancelamento</html>"))
            });

        var result = await service.CancelAsync(
            new CancelFiscalDocumentCommand(document.Id, "Cancelamento solicitado por ajuste operacional."),
            CancellationToken.None);

        Assert.Equal("Cancelled", result.Status);
        Assert.Contains(result.Events, x => x.EventType == "cancelled");
        Assert.Contains(result.Artifacts, x => x.Kind == "cancellation-xml");
        Assert.Equal("Cancelada", stateStore.FiscalInvoices.Single().Status);
    }

    [Fact]
    public async Task ApplyCorrectionLetterAsync_ShouldKeepDocumentAuthorized_AndAppendArtifacts()
    {
        var stateStore = new AppStateStore();
        var company = BuildCompany();
        var document = BuildAuthorizedDocument(company.Id);
        stateStore.FiscalCompanies.Add(company);
        stateStore.FiscalDocuments.Add(document);

        var service = CreateService(
            stateStore,
            new FakeFiscalEngine
            {
                CorrectHandler = _ => Task.FromResult(new FiscalNfeEventResult(
                    "CC-e registrada",
                    "13520260328120002",
                    "mock-plugavel",
                    "<evento>cce</evento>",
                    "<html>cce</html>"))
            });

        var result = await service.ApplyCorrectionLetterAsync(
            new ApplyFiscalCorrectionLetterCommand(document.Id, "Correcao do texto complementar do destinatario."),
            CancellationToken.None);

        Assert.Equal("Authorized", result.Status);
        Assert.Contains(result.Events, x => x.EventType == "correction_letter_registered");
        Assert.Contains(result.Artifacts, x => x.Kind == "correction-letter-xml");
    }

    [Fact]
    public async Task InutilizeNumberRangeAsync_ShouldRegisterNumberingEvent()
    {
        var stateStore = new AppStateStore();
        var company = BuildCompany();
        stateStore.FiscalCompanies.Add(company);

        var service = CreateService(
            stateStore,
            new FakeFiscalEngine
            {
                InutilizeHandler = _ => Task.FromResult(new FiscalNfeEventResult(
                    "Inutilizacao registrada",
                    "13520260328120003",
                    "mock-plugavel",
                    "<evento>inutilizacao</evento>",
                    "<html>inutilizacao</html>"))
            });

        var overview = await service.InutilizeNumberRangeAsync(
            new InutilizeFiscalNumberRangeCommand(
                company.Id,
                "3",
                120,
                121,
                "Faixa reservada sem uso operacional no periodo."),
            CancellationToken.None);

        var numberingEvent = Assert.Single(overview.NumberingEvents);
        Assert.Equal("3", numberingEvent.Series);
        Assert.Equal(120, numberingEvent.StartNumber);
        Assert.Equal(121, numberingEvent.EndNumber);
        Assert.Equal("Inutilized", numberingEvent.Status);
    }

    private static FiscalDocumentService CreateService(AppStateStore stateStore, FakeFiscalEngine engine)
        => new(
            stateStore,
            new FixedClock(new DateTime(2026, 03, 28, 12, 00, 00, DateTimeKind.Utc)),
            new FakeCurrentUserAccessor(),
            new InMemoryAppStatePersistence(),
            new InMemoryFileStorage(),
            engine);

    private static FiscalCompanyProfileState BuildCompany()
        => new()
        {
            Id = Guid.NewGuid(),
            TradeName = "PackControl Facaria Ltda",
            DocumentNumber = "12345678000195",
            StateRegistration = "123456789",
            TaxRegime = "Lucro Presumido",
            PostalCode = "01311-000",
            Street = "Avenida Paulista",
            StreetNumber = "1450",
            District = "Bela Vista",
            City = "Sao Paulo",
            StateCode = "SP",
            CityIbgeCode = "3550308",
            Country = "Brasil",
            Complement = null,
            FiscalSeries = "1",
            NfeEnabled = true,
            Environment = "Homologacao",
            AdapterName = "mock-plugavel",
            CertificateType = "A1",
            CertificateMedia = "Arquivo",
            CertificateLabel = "Certificado Demo",
            PrincipalEmissionMode = "A1",
            ContingencyEmissionMode = "A3",
            AccountantValidated = true,
            HomologationCredentialsValidated = true,
            HomologationApproved = false,
            ProductionCredentialsValidated = false,
            ProductionApproved = false,
            LastNfeNumber = 122
        };

    private static FiscalDocumentState BuildAuthorizedDocument(Guid companyId)
        => new()
        {
            Id = Guid.NewGuid(),
            CompanyProfileId = companyId,
            FinanceEntryId = null,
            OrderId = null,
            OrderNumber = "PC-000123",
            Number = "000000123",
            Series = "1",
            Environment = "Homologacao",
            AccessKey = "35123456789012345678901234567890123456789012",
            Protocol = "13520260327174001",
            AdapterName = "mock-plugavel",
            IssueMode = "A1 centralizado",
            CertificateType = "A1",
            CertificateMedia = "Arquivo",
            NatureOfOperation = "Venda de produto",
            Cfop = "5101",
            RecipientName = "Cliente Demo",
            RecipientDocument = "98765432000111",
            Amount = 100m,
            EmitterSnapshot = new FiscalEmitterSnapshotState
            {
                CompanyId = companyId,
                TradeName = "PackControl Facaria Ltda",
                DocumentNumber = "12345678000195",
                StateRegistration = "123456789",
                TaxRegime = "Lucro Presumido",
                FiscalSeries = "1",
                Environment = "Homologacao",
                Address = new FiscalAddressSnapshotState
                {
                    PostalCode = "01311-000",
                    Street = "Avenida Paulista",
                    StreetNumber = "1450",
                    District = "Bela Vista",
                    City = "Sao Paulo",
                    StateCode = "SP",
                    CityIbgeCode = "3550308",
                    Country = "Brasil"
                }
            },
            RecipientSnapshot = new FiscalRecipientSnapshotState
            {
                Name = "Cliente Demo",
                DocumentNumber = "98765432000111",
                StateRegistration = "123456789",
                TaxpayerIndicator = "Contribuinte",
                Email = "financeiro@cliente.local",
                Phone = "(11) 4000-0000",
                Address = new FiscalAddressSnapshotState
                {
                    PostalCode = "09540-500",
                    Street = "Rua Amazonas",
                    StreetNumber = "780",
                    District = "Centro",
                    City = "Sao Caetano do Sul",
                    StateCode = "SP",
                    CityIbgeCode = "3548807",
                    Country = "Brasil"
                }
            },
            Items =
            [
                new FiscalDocumentItemState
                {
                    LineNumber = 1,
                    Description = "Item demo",
                    CommercialUnit = "UN",
                    Quantity = 1m,
                    TaxQuantity = 1m,
                    UnitPrice = 100m,
                    GrossAmount = 100m,
                    DiscountAmount = 0m,
                    TotalAmount = 100m,
                    BillingMethod = "Por unidade",
                    Cfop = "5101",
                    Ncm = "8208.90.00",
                    OriginCode = "0",
                    IcmsSituationCode = "00",
                    IpiSituationCode = "99",
                    PisSituationCode = "49",
                    CofinsSituationCode = "49",
                    IcmsRate = 18m,
                    IcmsBaseAmount = 100m,
                    IcmsAmount = 18m,
                    IpiRate = 0m,
                    IpiAmount = 0m,
                    PisRate = 1.65m,
                    PisAmount = 1.65m,
                    CofinsRate = 7.6m,
                    CofinsAmount = 7.6m
                }
            ],
            Totals = new FiscalDocumentTotalsState
            {
                ProductsAmount = 100m,
                DiscountAmount = 0m,
                FreightAmount = 0m,
                InsuranceAmount = 0m,
                OtherAmount = 0m,
                IcmsBaseAmount = 100m,
                IcmsAmount = 18m,
                IpiAmount = 0m,
                PisAmount = 1.65m,
                CofinsAmount = 7.6m,
                InvoiceAmount = 100m
            },
            Payment = new FiscalDocumentPaymentState
            {
                PaymentMethod = "Boleto",
                BillingType = "A prazo",
                EntrySource = "Manual",
                BillingAmount = 100m,
                DueAtUtc = new DateTime(2026, 04, 04, 12, 00, 00, DateTimeKind.Utc),
                BoletoNumber = "23790001",
                BoletoLine = "23790 0001"
            },
            Transport = new FiscalDocumentTransportState
            {
                Mode = "Entrega terceirizada",
                FreightMode = "Terceiros",
                CarrierName = "Transportadora Demo",
                RecipientName = "Cliente Demo",
                DriverName = "Motorista",
                VehiclePlate = "ABC1D23"
            },
            Status = "Authorized",
            LastError = null,
            AttemptsCount = 1,
            XmlArchivePath = "/tmp/nfe.xml",
            DanfeArchivePath = "/tmp/danfe.html",
            CreatedAtUtc = new DateTime(2026, 03, 27, 17, 40, 00, DateTimeKind.Utc),
            IssuedAtUtc = new DateTime(2026, 03, 27, 17, 41, 00, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 03, 27, 17, 41, 00, DateTimeKind.Utc),
            Notes = "Observacao"
        };

    private sealed class FixedClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class FakeCurrentUserAccessor : ICurrentUserAccessor
    {
        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public string DisplayName => "CLI Fiscal";
        public UserRole? Role => UserRole.Administrator;
    }

    private sealed class InMemoryAppStatePersistence : IAppStatePersistence
    {
        public bool Enabled => false;
        public Task LoadAsync(AppStateStore stateStore, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveAsync(AppStateStore stateStore, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryFileStorage : IFileStorage
    {
        public async Task<StoredFileDescriptor> SaveAsync(
            Stream source,
            string originalFileName,
            string? contentType,
            CancellationToken cancellationToken)
        {
            using var buffer = new MemoryStream();
            await source.CopyToAsync(buffer, cancellationToken);

            return new StoredFileDescriptor(
                originalFileName,
                originalFileName,
                $"/tmp/{originalFileName}",
                contentType ?? "application/octet-stream",
                buffer.Length,
                $"sha-{originalFileName}");
        }

        public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
            => Task.FromResult<Stream>(new MemoryStream());
    }

    private sealed class FakeFiscalEngine : IFiscalNfeEngine
    {
        public Func<FiscalNfeEmissionRequest, Task<FiscalNfeEmissionResult>>? IssueHandler { get; init; }
        public Func<FiscalNfeCancellationRequest, Task<FiscalNfeEventResult>>? CancelHandler { get; init; }
        public Func<FiscalNfeCorrectionLetterRequest, Task<FiscalNfeEventResult>>? CorrectHandler { get; init; }
        public Func<FiscalNfeInutilizationRequest, Task<FiscalNfeEventResult>>? InutilizeHandler { get; init; }

        public Task<FiscalNfeEmissionResult> IssueAsync(FiscalNfeEmissionRequest request, CancellationToken cancellationToken)
            => IssueHandler?.Invoke(request)
               ?? Task.FromResult(new FiscalNfeEmissionResult("Autorizado", "chave", "protocolo", "mock-plugavel", "<xml />", "<html />"));

        public Task<FiscalNfeEventResult> CancelAsync(FiscalNfeCancellationRequest request, CancellationToken cancellationToken)
            => CancelHandler?.Invoke(request)
               ?? Task.FromResult(new FiscalNfeEventResult("Cancelamento registrado", "protocolo", "mock-plugavel", "<xml />", "<html />"));

        public Task<FiscalNfeEventResult> CorrectAsync(FiscalNfeCorrectionLetterRequest request, CancellationToken cancellationToken)
            => CorrectHandler?.Invoke(request)
               ?? Task.FromResult(new FiscalNfeEventResult("CC-e registrada", "protocolo", "mock-plugavel", "<xml />", "<html />"));

        public Task<FiscalNfeEventResult> InutilizeAsync(FiscalNfeInutilizationRequest request, CancellationToken cancellationToken)
            => InutilizeHandler?.Invoke(request)
               ?? Task.FromResult(new FiscalNfeEventResult("Inutilizacao registrada", "protocolo", "mock-plugavel", "<xml />", "<html />"));

        public Task<FiscalNfeStatusResult> CheckStatusAsync(FiscalNfeStatusRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new FiscalNfeStatusResult(
                request.AdapterName,
                "FakeEngine",
                true,
                true,
                true,
                107,
                "Operante",
                "Status simulado para testes.",
                null,
                null));
    }
}
