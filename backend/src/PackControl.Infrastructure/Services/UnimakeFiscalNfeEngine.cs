using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackControl.Application.Fiscal;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.NFe;
using Unimake.Business.DFe.Xml.NFe;

namespace PackControl.Infrastructure.Services;

public sealed class UnimakeFiscalNfeEngine(
    IOptions<UnimakeFiscalEngineOptions> options,
    ILogger<UnimakeFiscalNfeEngine> logger) : IFiscalNfeEngineAdapter
{
    private const string EventSchemaVersion = "1.00";
    private const string CorrectionLetterTerms =
        "A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do Convenio S/N, de 15 de dezembro de 1970 e pode ser utilizada para regularizacao de erro ocorrido na emissao de documento fiscal, desde que o erro nao esteja relacionado com: I - as variaveis que determinam o valor do imposto tais como: base de calculo, aliquota, diferenca de preco, quantidade, valor da operacao ou da prestacao; II - a correcao de dados cadastrais que implique mudanca do remetente ou do destinatario; III - a data de emissao ou de saida.";
    private readonly UnimakeFiscalEngineOptions options = options.Value;

    public string AdapterName => "unimake.dfe";

    public async Task<FiscalNfeEmissionResult> IssueAsync(FiscalNfeEmissionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureRealEmissionEnabled();

        var environment = MapEnvironment(request.Emitter.Environment);
        var stateCode = MapStateCode(request.Emitter.Address.StateCode);
        var enviNFe = UnimakeNfeXmlBuilder.Build(
            request,
            options.SchemaVersion,
            options.ProcessVersion);

        var configuration = BuildConfiguration(
            environment,
            stateCode,
            Servico.NFeAutorizacao,
            requireCertificate: true);

        var service = new Autorizacao(enviNFe, configuration);
        await Task.Run(service.Executar, cancellationToken);

        var outcome = await ResolveOutcomeAsync(service, configuration, request, cancellationToken);
        if (!IsAuthorizedStatus(outcome.StatusCode))
        {
            throw new InvalidOperationException(
                $"SEFAZ retornou rejeicao/nao autorizacao {outcome.StatusCode}: {outcome.StatusMessage}");
        }

        var xmlContent = ResolveDistributionXml(service, enviNFe);
        var danfeHtml = BuildDanfePreviewHtml(request, outcome);

        return new FiscalNfeEmissionResult(
            "Autorizado pela SEFAZ",
            outcome.AccessKey,
            outcome.Protocol,
            AdapterName,
            xmlContent,
            danfeHtml);
    }

    public async Task<FiscalNfeEventResult> CancelAsync(FiscalNfeCancellationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureRealEmissionEnabled();

        var environment = MapEnvironment(request.Emitter.Environment);
        var stateCode = MapStateCode(request.Emitter.Address.StateCode);
        var envelope = BuildCancellationEnvelope(request, environment, stateCode);
        var configuration = BuildConfiguration(
            environment,
            stateCode,
            Servico.NFeRecepcaoEvento,
            requireCertificate: true);

        var service = new RecepcaoEvento(envelope, configuration);
        await Task.Run(service.Executar, cancellationToken);

        var outcome = ResolveEventOutcome(service, request.AccessKey, "cancelamento");
        var xmlContent = ResolveEventDistributionXml(service, envelope);
        var previewHtml = BuildEventPreviewHtml(
            "Cancelamento registrado na SEFAZ",
            request.AccessKey,
            outcome.Protocol,
            outcome.StatusMessage,
            ("Justificativa", request.Justification));

        return new FiscalNfeEventResult(
            "Cancelamento homologado pela SEFAZ",
            outcome.Protocol,
            AdapterName,
            xmlContent,
            previewHtml);
    }

    public async Task<FiscalNfeEventResult> CorrectAsync(FiscalNfeCorrectionLetterRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureRealEmissionEnabled();

        var environment = MapEnvironment(request.Emitter.Environment);
        var stateCode = MapStateCode(request.Emitter.Address.StateCode);
        var envelope = BuildCorrectionLetterEnvelope(request, environment, stateCode);
        var configuration = BuildConfiguration(
            environment,
            stateCode,
            Servico.NFeRecepcaoEvento,
            requireCertificate: true);

        var service = new RecepcaoEvento(envelope, configuration);
        await Task.Run(service.Executar, cancellationToken);

        var outcome = ResolveEventOutcome(service, request.AccessKey, "CC-e");
        var xmlContent = ResolveEventDistributionXml(service, envelope);
        var previewHtml = BuildEventPreviewHtml(
            "Carta de correcao registrada na SEFAZ",
            request.AccessKey,
            outcome.Protocol,
            outcome.StatusMessage,
            ("Sequencial", request.SequenceNumber.ToString()),
            ("Correcao", request.CorrectionText));

        return new FiscalNfeEventResult(
            "CC-e homologada pela SEFAZ",
            outcome.Protocol,
            AdapterName,
            xmlContent,
            previewHtml);
    }

    public async Task<FiscalNfeEventResult> InutilizeAsync(FiscalNfeInutilizationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureRealEmissionEnabled();

        var environment = MapEnvironment(request.Emitter.Environment);
        var stateCode = MapStateCode(request.Emitter.Address.StateCode);
        var inutNFe = BuildInutilizationEnvelope(request, environment, stateCode);
        var configuration = BuildConfiguration(
            environment,
            stateCode,
            Servico.NFeInutilizacao,
            requireCertificate: true);

        var service = new Inutilizacao(inutNFe, configuration);
        await Task.Run(service.Executar, cancellationToken);

        var outcome = ResolveInutilizationOutcome(service, request);
        var xmlContent = ResolveInutilizationDistributionXml(service, inutNFe);
        var previewHtml = BuildEventPreviewHtml(
            "Inutilizacao registrada na SEFAZ",
            null,
            outcome.Protocol,
            outcome.StatusMessage,
            ("Serie", request.Series),
            ("Faixa", $"{request.StartNumber:000000000}-{request.EndNumber:000000000}"),
            ("Justificativa", request.Justification));

        return new FiscalNfeEventResult(
            "Inutilizacao homologada pela SEFAZ",
            outcome.Protocol,
            AdapterName,
            xmlContent,
            previewHtml);
    }

    public async Task<FiscalNfeStatusResult> CheckStatusAsync(FiscalNfeStatusRequest request, CancellationToken cancellationToken)
    {
        var hasCertificateMaterial = HasCertificateMaterial();
        var supportsRealEmission = options.AllowRealEmission &&
            (!request.RequireCertificate || hasCertificateMaterial);

        try
        {
            var environment = MapEnvironment(request.Environment);
            var stateCode = MapStateCode(request.StateCode);
            var configuration = BuildConfiguration(
                environment,
                stateCode,
                Servico.NFeStatusServico,
                options.UseCertificateForStatusService && request.RequireCertificate);

            var statusRequest = new ConsStatServ
            {
                Versao = options.SchemaVersion,
                TpAmb = environment,
                CUF = stateCode,
                XServ = "STATUS"
            };

            var service = new StatusServico(statusRequest, configuration);
            await Task.Run(service.Executar, cancellationToken);

            var result = service.Result;
            var isOperational = result?.CStat == 107;
            var statusCode = result?.CStat;
            var message = result?.XMotivo ?? "Consulta de status concluida sem mensagem.";

            if (request.RequireCertificate && !hasCertificateMaterial)
            {
                message = $"{message} Material de certificado nao configurado para emissao real.";
            }

            return new FiscalNfeStatusResult(
                AdapterName,
                "Unimake.DFe",
                true,
                isOperational,
                supportsRealEmission,
                statusCode,
                isOperational ? "Servico operante" : "Servico consultado",
                message,
                result?.VerAplic,
                service.RetornoWSString);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao consultar status do servico NF-e pelo adapter Unimake.");

            return new FiscalNfeStatusResult(
                AdapterName,
                "Unimake.DFe",
                false,
                false,
                supportsRealEmission,
                null,
                "Falha de consulta",
                request.RequireCertificate && !hasCertificateMaterial
                    ? $"{ex.Message} Material de certificado nao configurado para emissao real."
                    : ex.Message,
                null,
                null);
        }
    }

    private async Task<AuthorizationOutcome> ResolveOutcomeAsync(
        Autorizacao service,
        Configuracao configuration,
        FiscalNfeEmissionRequest request,
        CancellationToken cancellationToken)
    {
        var immediateProtocol = ResolveProtocolFromAuthorization(service.Result?.ProtNFe);
        if (immediateProtocol is not null)
        {
            return immediateProtocol;
        }

        if (service.Result?.CStat == 104)
        {
            throw new InvalidOperationException(
                $"Lote processado sem protocolo de autorizacao para a NF-e {request.Emitter.NfeNumber}: {service.Result.XMotivo}");
        }

        if (service.Result?.CStat != 103 || string.IsNullOrWhiteSpace(service.Result.InfRec?.NRec))
        {
            throw new InvalidOperationException(
                $"Falha na autorizacao NF-e {request.Emitter.NfeNumber}: {service.Result?.CStat} - {service.Result?.XMotivo ?? "Sem retorno do autorizador."}");
        }

        var receiptNumber = service.Result.InfRec.NRec;
        var pollDelayMs = Math.Max(250, options.ReceiptPollDelayMs);
        var pollAttempts = Math.Max(1, options.ReceiptPollMaxAttempts);

        for (var attempt = 1; attempt <= pollAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(pollDelayMs, cancellationToken);

            var receiptService = new RetAutorizacao(
                new ConsReciNFe
                {
                    Versao = options.SchemaVersion,
                    TpAmb = MapEnvironment(request.Emitter.Environment),
                    NRec = receiptNumber
                },
                BuildConfiguration(
                    MapEnvironment(request.Emitter.Environment),
                    MapStateCode(request.Emitter.Address.StateCode),
                    Servico.NFeConsultaRecibo,
                    requireCertificate: true));

            await Task.Run(receiptService.Executar, cancellationToken);

            var result = receiptService.Result;
            var protocol = result?.ProtNFe?
                .Select(ResolveProtocolFromAuthorization)
                .FirstOrDefault(x => x is not null);

            if (protocol is not null)
            {
                return protocol;
            }

            if (result?.CStat == 105)
            {
                continue;
            }

            throw new InvalidOperationException(
                $"Consulta de recibo {receiptNumber} retornou {result?.CStat}: {result?.XMotivo ?? "Sem mensagem do autorizador."}");
        }

        throw new InvalidOperationException(
            $"Recibo {receiptNumber} nao retornou protocolo de autorizacao apos {pollAttempts} tentativas.");
    }

    private static AuthorizationOutcome? ResolveProtocolFromAuthorization(ProtNFe? protocol)
    {
        var info = protocol?.InfProt;
        if (info is null)
        {
            return null;
        }

        return new AuthorizationOutcome(
            info.CStat,
            info.XMotivo ?? "Sem motivo retornado pelo autorizador.",
            info.ChNFe ?? string.Empty,
            info.NProt ?? string.Empty,
            info.VerAplic);
    }

    private string ResolveDistributionXml(Autorizacao service, EnviNFe envelope)
    {
        try
        {
            using var stream = new MemoryStream();
            service.GravarXmlDistribuicao(stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(content))
            {
                return content;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao extrair XML distribuicao oficial; usando fallback local do adapter.");
        }

        return service.NfeProcResult?.GerarXML().OuterXml
            ?? service.ConteudoXMLAssinado?.OuterXml
            ?? envelope.GerarXML().OuterXml;
    }

    private EnvEvento BuildCancellationEnvelope(
        FiscalNfeCancellationRequest request,
        TipoAmbiente environment,
        UFBrasil stateCode)
    {
        var detail = new DetEventoCanc
        {
            DescEvento = "Cancelamento",
            NProt = request.Protocol,
            XJust = request.Justification,
            Versao = EventSchemaVersion
        };

        return BuildEventEnvelope(
            request.AccessKey,
            request.Emitter.DocumentNumber,
            environment,
            stateCode,
            TipoEventoNFe.Cancelamento,
            1,
            detail);
    }

    private EnvEvento BuildCorrectionLetterEnvelope(
        FiscalNfeCorrectionLetterRequest request,
        TipoAmbiente environment,
        UFBrasil stateCode)
    {
        var detail = new DetEventoCCE
        {
            DescEvento = "Carta de Correcao",
            XCorrecao = request.CorrectionText,
            XCondUso = CorrectionLetterTerms,
            Versao = EventSchemaVersion
        };

        return BuildEventEnvelope(
            request.AccessKey,
            request.Emitter.DocumentNumber,
            environment,
            stateCode,
            TipoEventoNFe.CartaCorrecao,
            request.SequenceNumber,
            detail);
    }

    private InutNFe BuildInutilizationEnvelope(
        FiscalNfeInutilizationRequest request,
        TipoAmbiente environment,
        UFBrasil stateCode)
    {
        var documentNumber = NormalizeDigits(request.Emitter.DocumentNumber);
        var series = ParseSeries(request.Series);
        var year = DateTime.UtcNow.ToString("yy");
        var startNumber = request.StartNumber;
        var endNumber = request.EndNumber;

        return new InutNFe
        {
            Versao = options.SchemaVersion,
            InfInut = new InutNFeInfInut
            {
                TpAmb = environment,
                XServ = "INUTILIZAR",
                CUF = stateCode,
                Ano = year,
                CNPJ = documentNumber,
                Mod = ModeloDFe.NFe,
                Serie = series,
                NNFIni = startNumber,
                NNFFin = endNumber,
                XJust = request.Justification,
                Id = $"ID{(int)stateCode:00}{year}{documentNumber}{(int)ModeloDFe.NFe:00}{series:000}{startNumber:000000000}{endNumber:000000000}"
            }
        };
    }

    private static EnvEvento BuildEventEnvelope(
        string accessKey,
        string authorDocument,
        TipoAmbiente environment,
        UFBrasil stateCode,
        TipoEventoNFe eventType,
        int sequenceNumber,
        EventoDetalhe detail)
    {
        var info = new InfEvento(detail)
        {
            ChNFe = accessKey,
            COrgao = stateCode,
            TpAmb = environment,
            TpEvento = eventType,
            NSeqEvento = sequenceNumber,
            DhEvento = DateTimeOffset.UtcNow,
            VerEvento = EventSchemaVersion,
            Id = $"ID{(int)eventType:000000}{accessKey}{sequenceNumber:00}"
        };

        AssignAuthorDocument(info, authorDocument);

        return new EnvEvento
        {
            Versao = EventSchemaVersion,
            IdLote = BuildEventBatchId(),
            Evento =
            [
                new Evento
                {
                    Versao = EventSchemaVersion,
                    InfEvento = info
                }
            ]
        };
    }

    private EventOutcome ResolveEventOutcome(RecepcaoEvento service, string accessKey, string operationLabel)
    {
        var batchResult = service.Result
            ?? throw new InvalidOperationException($"SEFAZ nao retornou lote de {operationLabel} para a NF-e {accessKey}.");

        if (batchResult.CStat != 128)
        {
            throw new InvalidOperationException(
                $"SEFAZ retornou lote de {operationLabel} com status {batchResult.CStat}: {batchResult.XMotivo ?? "Sem mensagem do autorizador."}");
        }

        var eventInfo = batchResult.RetEvento?
            .Select(x => x.InfEvento)
            .FirstOrDefault(x => x is not null)
            ?? throw new InvalidOperationException($"SEFAZ processou o lote de {operationLabel}, mas nao retornou protocolo do evento.");

        if (!IsSuccessfulEventStatus(eventInfo.CStat))
        {
            throw new InvalidOperationException(
                $"SEFAZ retornou rejeicao/nao vinculacao do evento {eventInfo.CStat}: {eventInfo.XMotivo ?? "Sem mensagem do autorizador."}");
        }

        return new EventOutcome(
            eventInfo.CStat,
            eventInfo.XMotivo ?? "Evento processado pela SEFAZ.",
            eventInfo.NProt ?? string.Empty,
            eventInfo.DhRegEvento,
            batchResult.VerAplic ?? eventInfo.VerAplic);
    }

    private InutilizationOutcome ResolveInutilizationOutcome(Inutilizacao service, FiscalNfeInutilizationRequest request)
    {
        var result = service.Result
            ?? throw new InvalidOperationException(
                $"SEFAZ nao retornou resultado de inutilizacao para a faixa {request.Series}/{request.StartNumber:000000000}-{request.EndNumber:000000000}.");

        var info = result.InfInut
            ?? throw new InvalidOperationException("SEFAZ nao retornou o bloco infInut da inutilizacao.");

        if (!IsSuccessfulInutilizationStatus(info.CStat))
        {
            throw new InvalidOperationException(
                $"SEFAZ retornou rejeicao/nao homologacao da inutilizacao {info.CStat}: {info.XMotivo ?? "Sem mensagem do autorizador."}");
        }

        return new InutilizationOutcome(
            info.CStat,
            info.XMotivo ?? "Inutilizacao processada pela SEFAZ.",
            info.NProt ?? string.Empty,
            info.DhRecbto,
            info.VerAplic);
    }

    private string ResolveEventDistributionXml(RecepcaoEvento service, EnvEvento envelope)
    {
        try
        {
            using var stream = new MemoryStream();
            service.GravarXmlDistribuicao(stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(content))
            {
                return content;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao extrair XML distribuicao do evento; usando fallback local do adapter.");
        }

        return service.ProcEventoNFeResult?.FirstOrDefault()?.GerarXML().OuterXml
            ?? service.ConteudoXMLAssinado?.OuterXml
            ?? envelope.GerarXML().OuterXml;
    }

    private string ResolveInutilizationDistributionXml(Inutilizacao service, InutNFe request)
    {
        try
        {
            using var stream = new MemoryStream();
            service.GravarXmlDistribuicao(stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(content))
            {
                return content;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao extrair XML distribuicao da inutilizacao; usando fallback local do adapter.");
        }

        return service.ProcInutNFeResult?.GerarXML().OuterXml
            ?? service.ConteudoXMLAssinado?.OuterXml
            ?? request.GerarXML().OuterXml;
    }

    private string BuildEventPreviewHtml(
        string title,
        string? accessKey,
        string protocol,
        string statusMessage,
        params (string Label, string Value)[] lines)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"pt-BR\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\" />");
        builder.AppendLine("  <title>Espelho fiscal de evento</title>");
        builder.AppendLine("  <style>");
        builder.AppendLine("    body { font-family: sans-serif; margin: 24px; color: #1f2937; }");
        builder.AppendLine("    h1 { margin: 0 0 12px; }");
        builder.AppendLine("    .meta { border: 1px solid #d1d5db; padding: 12px; border-radius: 8px; display: grid; gap: 8px; }");
        builder.AppendLine("    small { color: #6b7280; }");
        builder.AppendLine("  </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine($"  <h1>{Encode(title)}</h1>");
        builder.AppendLine("  <p><small>Representacao operacional simplificada do evento. O XML protocolado permanece como artefato canonico.</small></p>");
        builder.AppendLine("  <div class=\"meta\">");
        if (!string.IsNullOrWhiteSpace(accessKey))
        {
            builder.AppendLine($"    <div><strong>Chave:</strong> {Encode(accessKey)}</div>");
        }
        builder.AppendLine($"    <div><strong>Protocolo:</strong> {Encode(protocol)}</div>");
        builder.AppendLine($"    <div><strong>Status:</strong> {Encode(statusMessage)}</div>");
        foreach (var line in lines)
        {
            builder.AppendLine($"    <div><strong>{Encode(line.Label)}:</strong> {Encode(line.Value)}</div>");
        }
        builder.AppendLine("  </div>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }

    private string BuildDanfePreviewHtml(FiscalNfeEmissionRequest request, AuthorizationOutcome outcome)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"pt-BR\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\" />");
        builder.AppendLine("  <title>Espelho fiscal NF-e</title>");
        builder.AppendLine("  <style>");
        builder.AppendLine("    body { font-family: sans-serif; margin: 24px; color: #1f2937; }");
        builder.AppendLine("    h1, h2 { margin: 0 0 12px; }");
        builder.AppendLine("    .meta, .box { border: 1px solid #d1d5db; padding: 12px; margin-bottom: 16px; border-radius: 8px; }");
        builder.AppendLine("    table { width: 100%; border-collapse: collapse; }");
        builder.AppendLine("    th, td { border-bottom: 1px solid #e5e7eb; padding: 8px; text-align: left; }");
        builder.AppendLine("    small { color: #6b7280; }");
        builder.AppendLine("  </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("  <h1>NF-e autorizada no SEFAZ</h1>");
        builder.AppendLine("  <p><small>Representacao simplificada para conferência operacional. O XML autorizado permanece como artefato canonico.</small></p>");
        builder.AppendLine("  <div class=\"meta\">");
        builder.AppendLine($"    <div><strong>Chave:</strong> {Encode(outcome.AccessKey)}</div>");
        builder.AppendLine($"    <div><strong>Protocolo:</strong> {Encode(outcome.Protocol)}</div>");
        builder.AppendLine($"    <div><strong>Status:</strong> {Encode(outcome.StatusMessage)}</div>");
        builder.AppendLine($"    <div><strong>Natureza:</strong> {Encode(request.NatureOfOperation)}</div>");
        builder.AppendLine("  </div>");
        builder.AppendLine("  <div class=\"box\">");
        builder.AppendLine($"    <h2>Emitente</h2><div>{Encode(request.Emitter.TradeName)} · {Encode(request.Emitter.DocumentNumber)}</div>");
        builder.AppendLine($"    <div>{Encode(request.Emitter.Address.Street)}, {Encode(request.Emitter.Address.StreetNumber)} · {Encode(request.Emitter.Address.City)}/{Encode(request.Emitter.Address.StateCode)}</div>");
        builder.AppendLine("  </div>");
        builder.AppendLine("  <div class=\"box\">");
        builder.AppendLine($"    <h2>Destinatario</h2><div>{Encode(request.Recipient.Name)} · {Encode(request.Recipient.DocumentNumber)}</div>");
        builder.AppendLine($"    <div>{Encode(request.Recipient.Address.Street)}, {Encode(request.Recipient.Address.StreetNumber)} · {Encode(request.Recipient.Address.City)}/{Encode(request.Recipient.Address.StateCode)}</div>");
        builder.AppendLine("  </div>");
        builder.AppendLine("  <table>");
        builder.AppendLine("    <thead><tr><th>Item</th><th>CFOP</th><th>NCM</th><th>Qtd.</th><th>Total</th></tr></thead>");
        builder.AppendLine("    <tbody>");

        foreach (var item in request.Items)
        {
            builder.AppendLine(
                $"      <tr><td>{Encode(item.Description)}</td><td>{Encode(item.Cfop)}</td><td>{Encode(item.Ncm)}</td><td>{item.Quantity:0.####}</td><td>R$ {item.TotalAmount:0.00}</td></tr>");
        }

        builder.AppendLine("    </tbody>");
        builder.AppendLine("  </table>");
        builder.AppendLine("  <div class=\"box\">");
        builder.AppendLine($"    <div><strong>Total produtos:</strong> R$ {request.Totals.ProductsAmount:0.00}</div>");
        builder.AppendLine($"    <div><strong>Total NF-e:</strong> R$ {request.Totals.InvoiceAmount:0.00}</div>");
        builder.AppendLine($"    <div><strong>Pagamento:</strong> {Encode(request.Payment.PaymentMethod)} · {Encode(request.Payment.BillingType)}</div>");
        builder.AppendLine("  </div>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }

    private void EnsureRealEmissionEnabled()
    {
        if (!options.AllowRealEmission)
        {
            throw new InvalidOperationException(
                "Adapter Unimake configurado, mas a emissao real esta bloqueada na configuracao. Ajuste FiscalEngines:Unimake:AllowRealEmission somente depois de validar certificado, ambiente e autorizador.");
        }
    }

    private Configuracao BuildConfiguration(
        TipoAmbiente environment,
        UFBrasil stateCode,
        Servico serviceType,
        bool requireCertificate)
    {
        var configuration = new Configuracao
        {
            TipoDFe = TipoDFe.NFe,
            Servico = serviceType,
            TipoAmbiente = environment,
            CodigoUF = (int)stateCode,
            SchemaVersao = options.SchemaVersion,
            TipoEmissao = TipoEmissao.Normal,
            UsaCertificadoDigital = requireCertificate
        };

        if (!string.IsNullOrWhiteSpace(options.HostHomologacao))
        {
            configuration.HostHomologacao = options.HostHomologacao;
        }

        if (!string.IsNullOrWhiteSpace(options.HostProducao))
        {
            configuration.HostProducao = options.HostProducao;
        }

        if (!string.IsNullOrWhiteSpace(options.RequestUriHomologacao))
        {
            configuration.RequestURIHomologacao = options.RequestUriHomologacao;
        }

        if (!string.IsNullOrWhiteSpace(options.RequestUriProducao))
        {
            configuration.RequestURIProducao = options.RequestUriProducao;
        }

        if (!string.IsNullOrWhiteSpace(options.WebEnderecoHomologacao))
        {
            configuration.WebEnderecoHomologacao = options.WebEnderecoHomologacao;
        }

        if (!string.IsNullOrWhiteSpace(options.WebEnderecoProducao))
        {
            configuration.WebEnderecoProducao = options.WebEnderecoProducao;
        }

        if (requireCertificate)
        {
            ApplyCertificateConfiguration(configuration);
        }

        return configuration;
    }

    private void ApplyCertificateConfiguration(Configuracao configuration)
    {
        if (!string.IsNullOrWhiteSpace(options.CertificatePath))
        {
            configuration.CertificadoArquivo = options.CertificatePath;
            configuration.CertificadoSenha = options.CertificatePassword;
            return;
        }

        if (!string.IsNullOrWhiteSpace(options.CertificateBase64))
        {
            configuration.CertificadoBase64 = options.CertificateBase64;
            configuration.CertificadoSenha = options.CertificatePassword;
            return;
        }

        if (!string.IsNullOrWhiteSpace(options.CertificateThumbprint))
        {
            configuration.CertificadoSerialNumberOrThumbPrint = options.CertificateThumbprint;
            return;
        }

        throw new InvalidOperationException(
            "Emissao real configurada, mas nenhum CertificatePath, CertificateBase64 ou CertificateThumbprint foi informado para o adapter Unimake.");
    }

    private bool HasCertificateMaterial()
        => !string.IsNullOrWhiteSpace(options.CertificatePath) ||
            !string.IsNullOrWhiteSpace(options.CertificateBase64) ||
            !string.IsNullOrWhiteSpace(options.CertificateThumbprint);

    private TipoAmbiente MapEnvironment(string? environment)
        => Normalize(environment) == "producao"
            ? TipoAmbiente.Producao
            : TipoAmbiente.Homologacao;

    private UFBrasil MapStateCode(string? stateCode)
    {
        var normalized = Normalize(stateCode) ?? Normalize(options.DefaultStateCode) ?? "sp";
        return normalized switch
        {
            "ro" => UFBrasil.RO,
            "ac" => UFBrasil.AC,
            "am" => UFBrasil.AM,
            "rr" => UFBrasil.RR,
            "pa" => UFBrasil.PA,
            "ap" => UFBrasil.AP,
            "to" => UFBrasil.TO,
            "ma" => UFBrasil.MA,
            "pi" => UFBrasil.PI,
            "ce" => UFBrasil.CE,
            "rn" => UFBrasil.RN,
            "pb" => UFBrasil.PB,
            "pe" => UFBrasil.PE,
            "al" => UFBrasil.AL,
            "se" => UFBrasil.SE,
            "ba" => UFBrasil.BA,
            "mg" => UFBrasil.MG,
            "es" => UFBrasil.ES,
            "rj" => UFBrasil.RJ,
            "sp" => UFBrasil.SP,
            "pr" => UFBrasil.PR,
            "sc" => UFBrasil.SC,
            "rs" => UFBrasil.RS,
            "ms" => UFBrasil.MS,
            "mt" => UFBrasil.MT,
            "go" => UFBrasil.GO,
            "df" => UFBrasil.DF,
            _ => throw new InvalidOperationException($"UF '{stateCode}' nao e suportada pelo adapter Unimake.")
        };
    }

    private static bool IsAuthorizedStatus(int statusCode) => statusCode is 100 or 150;

    private static bool IsSuccessfulEventStatus(int statusCode) => statusCode is 135 or 136 or 155;

    private static bool IsSuccessfulInutilizationStatus(int statusCode) => statusCode == 102;

    private static int ParseSeries(string? value)
    {
        if (!int.TryParse(value, out var parsed) || parsed < 0)
        {
            throw new InvalidOperationException($"Serie fiscal '{value}' nao e valida para o adapter Unimake.");
        }

        return parsed;
    }

    private static void AssignAuthorDocument(InfEvento info, string documentNumber)
    {
        var digits = NormalizeDigits(documentNumber);
        if (digits.Length == 14)
        {
            info.CNPJ = digits;
            return;
        }

        if (digits.Length == 11)
        {
            info.CPF = digits;
            return;
        }

        throw new InvalidOperationException("Documento do emitente precisa ter 11 ou 14 digitos para envio de evento na SEFAZ.");
    }

    private static string NormalizeDigits(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digits))
        {
            throw new InvalidOperationException("Documento fiscal obrigatorio nao foi informado.");
        }

        return digits;
    }

    private static string BuildEventBatchId()
        => $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1_000_000_000_000_000:000000000000000}";

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string? Normalize(string? value) => value?.Trim().ToLowerInvariant();

    private sealed record AuthorizationOutcome(
        int StatusCode,
        string StatusMessage,
        string AccessKey,
        string Protocol,
        string? ApplicationVersion);

    private sealed record EventOutcome(
        int StatusCode,
        string StatusMessage,
        string Protocol,
        DateTimeOffset RegisteredAt,
        string? ApplicationVersion);

    private sealed record InutilizationOutcome(
        int StatusCode,
        string StatusMessage,
        string Protocol,
        DateTimeOffset RegisteredAt,
        string? ApplicationVersion);
}
