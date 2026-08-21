using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PackControl.Application.Fiscal;
using PackControl.Infrastructure.Services;

namespace PackControl.Api.Tests;

public sealed class FiscalEngineTests
{
    [Fact]
    public async Task MockAdapter_Diagnostic_ShouldReturnSimulatedStatus()
    {
        var engine = new MockFiscalNfeEngine();

        var result = await engine.CheckStatusAsync(
            new FiscalNfeStatusRequest("mock-plugavel", "Homologacao", "SP", false),
            CancellationToken.None);

        Assert.Equal("mock-plugavel", result.AdapterName);
        Assert.Equal("Simulado", result.Status);
        Assert.True(result.IsReachable);
        Assert.False(result.SupportsRealEmission);
    }

    [Fact]
    public async Task RoutingEngine_ShouldRouteIssueByAdapterName()
    {
        var engine = new RoutingFiscalNfeEngine([new MockFiscalNfeEngine()]);

        var result = await engine.IssueAsync(BuildRequest("mock-plugavel"), CancellationToken.None);

        Assert.Equal("mock-plugavel", result.EngineName);
        Assert.Equal("Pronta para adaptador fiscal", result.Status);
    }

    [Fact]
    public async Task MockAdapter_Cancel_ShouldReturnEventPayload()
    {
        var engine = new MockFiscalNfeEngine();
        var request = BuildRequest("mock-plugavel");

        var result = await engine.CancelAsync(
            new FiscalNfeCancellationRequest(
                Guid.NewGuid(),
                "35123456789012345678901234567890123456789012",
                "13520260327174001",
                request.Emitter,
                "Cancelamento motivado por ajuste operacional."),
            CancellationToken.None);

        Assert.Equal("mock-plugavel", result.EngineName);
        Assert.Contains("cancelamento", result.XmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ajuste operacional", result.DisplayHtmlContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MockAdapter_Inutilize_ShouldReturnEventPayload()
    {
        var engine = new MockFiscalNfeEngine();
        var request = BuildRequest("mock-plugavel");

        var result = await engine.InutilizeAsync(
            new FiscalNfeInutilizationRequest(
                Guid.NewGuid(),
                request.Emitter,
                "3",
                120,
                121,
                "Faixa reservada sem uso operacional."),
            CancellationToken.None);

        Assert.Equal("mock-plugavel", result.EngineName);
        Assert.Contains("inutilizacao", result.XmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Faixa reservada", result.DisplayHtmlContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnimakeAdapter_Issue_ShouldBlockWhenRealEmissionIsDisabled()
    {
        var engine = new UnimakeFiscalNfeEngine(
            Options.Create(new UnimakeFiscalEngineOptions()),
            NullLogger<UnimakeFiscalNfeEngine>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.IssueAsync(BuildRequest("unimake.dfe"), CancellationToken.None));

        Assert.Contains("emissao real", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnimakeAdapter_Issue_ShouldRequireCertificateConfiguration_WhenRealEmissionIsEnabled()
    {
        var engine = new UnimakeFiscalNfeEngine(
            Options.Create(new UnimakeFiscalEngineOptions
            {
                AllowRealEmission = true
            }),
            NullLogger<UnimakeFiscalNfeEngine>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.IssueAsync(BuildRequest("unimake.dfe"), CancellationToken.None));

        Assert.Contains("CertificatePath", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnimakeAdapter_Cancel_ShouldBlockWhenRealEmissionIsDisabled()
    {
        var engine = new UnimakeFiscalNfeEngine(
            Options.Create(new UnimakeFiscalEngineOptions()),
            NullLogger<UnimakeFiscalNfeEngine>.Instance);
        var request = BuildRequest("unimake.dfe");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.CancelAsync(
                new FiscalNfeCancellationRequest(
                    Guid.NewGuid(),
                    "35123456789012345678901234567890123456789012",
                    "13520260327174001",
                    request.Emitter,
                    "Cancelamento motivado por ajuste operacional."),
                CancellationToken.None));

        Assert.Contains("emissao real", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnimakeAdapter_Correct_ShouldRequireCertificateConfiguration_WhenRealEmissionIsEnabled()
    {
        var engine = new UnimakeFiscalNfeEngine(
            Options.Create(new UnimakeFiscalEngineOptions
            {
                AllowRealEmission = true
            }),
            NullLogger<UnimakeFiscalNfeEngine>.Instance);
        var request = BuildRequest("unimake.dfe");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.CorrectAsync(
                new FiscalNfeCorrectionLetterRequest(
                    Guid.NewGuid(),
                    "35123456789012345678901234567890123456789012",
                    "13520260327174001",
                    request.Emitter,
                    request.Recipient,
                    1,
                    "Correcao do complemento do destinatario."),
                CancellationToken.None));

        Assert.Contains("CertificatePath", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnimakeAdapter_Inutilize_ShouldRequireCertificateConfiguration_WhenRealEmissionIsEnabled()
    {
        var engine = new UnimakeFiscalNfeEngine(
            Options.Create(new UnimakeFiscalEngineOptions
            {
                AllowRealEmission = true
            }),
            NullLogger<UnimakeFiscalNfeEngine>.Instance);
        var request = BuildRequest("unimake.dfe");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.InutilizeAsync(
                new FiscalNfeInutilizationRequest(
                    Guid.NewGuid(),
                    request.Emitter,
                    "3",
                    120,
                    121,
                    "Faixa reservada sem uso operacional."),
                CancellationToken.None));

        Assert.Contains("CertificatePath", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnimakeXmlBuilder_ShouldGenerateNfe55EnvelopeFromCanonicalSnapshot()
    {
        var envelope = UnimakeNfeXmlBuilder.Build(
            BuildRequest("unimake.dfe"),
            "4.00",
            "PackControl Test");

        var xml = envelope.GerarXML().OuterXml;

        Assert.Contains("enviNFe", xml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<mod>55</mod>", xml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<cMun>3550308</cMun>", xml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<cMun>3548807</cMun>", xml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<pag>", xml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<xProd>Item demo</xProd>", xml, StringComparison.OrdinalIgnoreCase);
    }

    private static FiscalNfeEmissionRequest BuildRequest(string adapterName)
        => new(
            Guid.NewGuid(),
            new FiscalEmitterProfile(
                Guid.NewGuid(),
                "PackControl Facaria Ltda",
                "12345678000195",
                "123456789",
                "Lucro Presumido",
                "3",
                "Homologacao",
                adapterName,
                new FiscalCertificateProfile("A1", "Arquivo", "Certificado Demo", null),
                new FiscalPartyAddress(
                    "01311-000",
                    "Avenida Paulista",
                    "1450",
                    "Bela Vista",
                    "Sao Paulo",
                    "SP",
                    "3550308",
                    "Brasil",
                    null,
                    null),
                121),
            new FiscalRecipientProfile(
                Guid.NewGuid(),
                "Cliente Demo",
                "98765432000111",
                "123456789",
                "Contribuinte",
                "financeiro@cliente.local",
                "(11) 4000-0000",
                new FiscalPartyAddress(
                    "09540-500",
                    "Rua Amazonas",
                    "780",
                    "Centro",
                    "Sao Caetano do Sul",
                    "SP",
                    "3548807",
                    "Brasil",
                    null,
                    null)),
            "Venda de produto",
            "5101",
            [
                new FiscalNfeItem(
                    1,
                    Guid.NewGuid(),
                    "Item demo",
                    "UN",
                    1m,
                    1m,
                    100m,
                    100m,
                    0m,
                    100m,
                    "Por unidade",
                    "5101",
                    "8208.90.00",
                    "0",
                    "00",
                    "99",
                    "49",
                    "49",
                    18m,
                    100m,
                    18m,
                    0m,
                    0m,
                    1.65m,
                    1.65m,
                    7.6m,
                    7.6m,
                    null),
                new FiscalNfeItem(
                    2,
                    Guid.NewGuid(),
                    "Montagem tecnica",
                    "HR",
                    2m,
                    2m,
                    50m,
                    100m,
                    0m,
                    100m,
                    "Por hora tecnica",
                    "5101",
                    "9985.19.90",
                    "0",
                    "41",
                    "99",
                    "49",
                    "49",
                    0m,
                    100m,
                    0m,
                    0m,
                    0m,
                    0.65m,
                    0.65m,
                    3m,
                    3m,
                    null)
            ],
            new FiscalNfeTotals(200m, 0m, 0m, 0m, 0m, 200m, 18m, 0m, 2.3m, 10.6m, 200m),
            new FiscalNfePayment("Boleto", "A prazo", "Manual", 200m, DateTime.UtcNow.Date.AddDays(7), "2379", "2379 0000"),
            new FiscalNfeTransport(Guid.NewGuid(), Guid.NewGuid(), "Transportadora Demo", "Entrega terceirizada", "Terceiros", "Recepcao", "Motorista", "ABC1D23", DateTime.UtcNow),
            "Observacao");
}
