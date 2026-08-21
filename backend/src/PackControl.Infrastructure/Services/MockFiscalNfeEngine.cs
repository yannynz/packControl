using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using PackControl.Application.Fiscal;

namespace PackControl.Infrastructure.Services;

public sealed class MockFiscalNfeEngine : IFiscalNfeEngineAdapter
{
    public string AdapterName => "mock-plugavel";

    public Task<FiscalNfeEmissionResult> IssueAsync(FiscalNfeEmissionRequest request, CancellationToken cancellationToken)
    {
        var accessKey = BuildAccessKey(request);
        var protocol = $"135{DateTime.UtcNow:yyyyMMddHHmmss}";

        var xml = new XDocument(
            new XElement("PackControlNFe",
                new XAttribute("ambiente", request.Emitter.Environment),
                new XAttribute("adaptador", "mock-plugavel"),
                new XElement("Emitente",
                    new XElement("RazaoSocial", request.Emitter.TradeName),
                    new XElement("Cnpj", request.Emitter.DocumentNumber),
                    new XElement("Ie", request.Emitter.StateRegistration),
                    new XElement("Regime", request.Emitter.TaxRegime),
                    new XElement("Serie", request.Emitter.FiscalSeries),
                    new XElement("Numero", request.Emitter.NfeNumber),
                    new XElement("Endereco",
                        new XElement("Cep", request.Emitter.Address.PostalCode),
                        new XElement("Logradouro", request.Emitter.Address.Street),
                        new XElement("Numero", request.Emitter.Address.StreetNumber),
                        new XElement("Bairro", request.Emitter.Address.District),
                        new XElement("Cidade", request.Emitter.Address.City),
                        new XElement("Uf", request.Emitter.Address.StateCode))),
                new XElement("Destinatario",
                    new XElement("Nome", request.Recipient.Name),
                    new XElement("Documento", request.Recipient.DocumentNumber ?? string.Empty),
                    new XElement("Ie", request.Recipient.StateRegistration ?? string.Empty),
                    new XElement("IndicadorIe", request.Recipient.TaxpayerIndicator),
                    new XElement("Endereco",
                        new XElement("Cep", request.Recipient.Address.PostalCode),
                        new XElement("Logradouro", request.Recipient.Address.Street),
                        new XElement("Numero", request.Recipient.Address.StreetNumber),
                        new XElement("Bairro", request.Recipient.Address.District),
                        new XElement("Cidade", request.Recipient.Address.City),
                        new XElement("Uf", request.Recipient.Address.StateCode))),
                new XElement("Operacao",
                    new XElement("Natureza", request.NatureOfOperation),
                    new XElement("Cfop", request.Cfop)),
                new XElement("Itens",
                    request.Items.Select(item =>
                        new XElement("Item",
                            new XAttribute("seq", item.LineNumber),
                            new XElement("Descricao", item.Description),
                            new XElement("Quantidade", item.Quantity),
                            new XElement("Unidade", item.CommercialUnit),
                            new XElement("ValorUnitario", item.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture)),
                            new XElement("ValorBruto", item.GrossAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                            new XElement("ValorTotal", item.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                            new XElement("Cfop", item.Cfop),
                            new XElement("Ncm", item.Ncm),
                            new XElement("Tributacao",
                                new XElement("Origem", item.OriginCode),
                                new XElement("CstIcms", item.IcmsSituationCode),
                                new XElement("AliquotaIcms", item.IcmsRate.ToString("0.00", CultureInfo.InvariantCulture)),
                                new XElement("ValorIcms", item.IcmsAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                                new XElement("CstIpi", item.IpiSituationCode),
                                new XElement("ValorIpi", item.IpiAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                                new XElement("CstPis", item.PisSituationCode),
                                new XElement("ValorPis", item.PisAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                                new XElement("CstCofins", item.CofinsSituationCode),
                                new XElement("ValorCofins", item.CofinsAmount.ToString("0.00", CultureInfo.InvariantCulture))),
                            new XElement("Observacoes", item.Notes ?? string.Empty)))),
                new XElement("Totais",
                    new XElement("Produtos", request.Totals.ProductsAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("Desconto", request.Totals.DiscountAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("Outros", request.Totals.OtherAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("Nota", request.Totals.InvoiceAmount.ToString("0.00", CultureInfo.InvariantCulture))),
                new XElement("Pagamento",
                    new XElement("Metodo", request.Payment.PaymentMethod),
                    new XElement("Tipo", request.Payment.BillingType),
                    new XElement("Valor", request.Payment.BillingAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("Boleto", request.Payment.BoletoNumber ?? string.Empty)),
                new XElement("Transporte",
                    new XElement("Modal", request.Transport.Mode),
                    new XElement("Frete", request.Transport.FreightMode),
                    new XElement("Transportadora", request.Transport.CarrierName ?? string.Empty)),
                new XElement("ChaveAcesso", accessKey),
                new XElement("Protocolo", protocol),
                new XElement("InformacoesAdicionais", request.AdditionalInformation ?? string.Empty)));

        var danfeHtml =
            $"""
             <html lang="pt-BR">
             <head><meta charset="utf-8"><title>DANFE {request.Emitter.DocumentNumber}</title></head>
             <body>
               <h1>DANFE preliminar PackControl</h1>
               <p>Adaptador: mock-plugavel</p>
               <p>Emitente: {request.Emitter.TradeName} ({request.Emitter.DocumentNumber})</p>
               <p>Destinatario: {request.Recipient.Name}</p>
               <p>Natureza: {request.NatureOfOperation}</p>
               <p>CFOP: {request.Cfop}</p>
               <p>Numero: {request.Emitter.NfeNumber} / Serie {request.Emitter.FiscalSeries}</p>
               <p>Chave: {accessKey}</p>
               <p>Protocolo: {protocol}</p>
               <p>Total: R$ {request.Totals.InvoiceAmount:0.00}</p>
             </body>
             </html>
             """;

        return Task.FromResult(new FiscalNfeEmissionResult(
            "Pronta para adaptador fiscal",
            accessKey,
            protocol,
            "mock-plugavel",
            xml.ToString(SaveOptions.DisableFormatting),
            danfeHtml));
    }

    public Task<FiscalNfeEventResult> CancelAsync(FiscalNfeCancellationRequest request, CancellationToken cancellationToken)
    {
        var protocol = $"135{DateTime.UtcNow:yyyyMMddHHmmss}";
        var xml = new XDocument(
            new XElement("PackControlNFeEvento",
                new XAttribute("tipo", "cancelamento"),
                new XAttribute("adaptador", AdapterName),
                new XElement("ChaveAcesso", request.AccessKey),
                new XElement("ProtocoloOrigem", request.Protocol),
                new XElement("ProtocoloEvento", protocol),
                new XElement("Justificativa", request.Justification),
                new XElement("Emitente",
                    new XElement("RazaoSocial", request.Emitter.TradeName),
                    new XElement("Cnpj", request.Emitter.DocumentNumber))));

        var preview =
            $"""
             <html lang="pt-BR">
             <head><meta charset="utf-8"><title>Cancelamento NF-e</title></head>
             <body>
               <h1>Cancelamento registrado</h1>
               <p>Adaptador: {AdapterName}</p>
               <p>Chave: {request.AccessKey}</p>
               <p>Protocolo do evento: {protocol}</p>
               <p>Justificativa: {request.Justification}</p>
             </body>
             </html>
             """;

        return Task.FromResult(new FiscalNfeEventResult(
            "Cancelamento registrado",
            protocol,
            AdapterName,
            xml.ToString(SaveOptions.DisableFormatting),
            preview));
    }

    public Task<FiscalNfeEventResult> CorrectAsync(FiscalNfeCorrectionLetterRequest request, CancellationToken cancellationToken)
    {
        var protocol = $"135{DateTime.UtcNow:yyyyMMddHHmmss}";
        var xml = new XDocument(
            new XElement("PackControlNFeEvento",
                new XAttribute("tipo", "cce"),
                new XAttribute("adaptador", AdapterName),
                new XElement("ChaveAcesso", request.AccessKey),
                new XElement("ProtocoloOrigem", request.Protocol),
                new XElement("ProtocoloEvento", protocol),
                new XElement("SequencialEvento", request.SequenceNumber),
                new XElement("TextoCorrecao", request.CorrectionText),
                new XElement("Emitente",
                    new XElement("RazaoSocial", request.Emitter.TradeName),
                    new XElement("Cnpj", request.Emitter.DocumentNumber)),
                new XElement("Destinatario",
                    new XElement("Nome", request.Recipient.Name),
                    new XElement("Documento", request.Recipient.DocumentNumber ?? string.Empty))));

        var preview =
            $"""
             <html lang="pt-BR">
             <head><meta charset="utf-8"><title>CC-e NF-e</title></head>
             <body>
               <h1>Carta de correcao registrada</h1>
               <p>Adaptador: {AdapterName}</p>
               <p>Chave: {request.AccessKey}</p>
               <p>Protocolo do evento: {protocol}</p>
               <p>Sequencial: {request.SequenceNumber}</p>
               <p>Correcao: {request.CorrectionText}</p>
             </body>
             </html>
             """;

        return Task.FromResult(new FiscalNfeEventResult(
            "CC-e registrada",
            protocol,
            AdapterName,
            xml.ToString(SaveOptions.DisableFormatting),
            preview));
    }

    public Task<FiscalNfeEventResult> InutilizeAsync(FiscalNfeInutilizationRequest request, CancellationToken cancellationToken)
    {
        var protocol = $"135{DateTime.UtcNow:yyyyMMddHHmmss}";
        var xml = new XDocument(
            new XElement("PackControlNFeEvento",
                new XAttribute("tipo", "inutilizacao"),
                new XAttribute("adaptador", AdapterName),
                new XElement("Emitente",
                    new XElement("RazaoSocial", request.Emitter.TradeName),
                    new XElement("Cnpj", request.Emitter.DocumentNumber)),
                new XElement("Serie", request.Series),
                new XElement("NumeroInicial", request.StartNumber.ToString("000000000", CultureInfo.InvariantCulture)),
                new XElement("NumeroFinal", request.EndNumber.ToString("000000000", CultureInfo.InvariantCulture)),
                new XElement("ProtocoloEvento", protocol),
                new XElement("Justificativa", request.Justification)));

        var preview =
            $"""
             <html lang="pt-BR">
             <head><meta charset="utf-8"><title>Inutilizacao NF-e</title></head>
             <body>
               <h1>Faixa inutilizada</h1>
               <p>Adaptador: {AdapterName}</p>
               <p>Serie: {request.Series}</p>
               <p>Faixa: {request.StartNumber:000000000} ate {request.EndNumber:000000000}</p>
               <p>Protocolo do evento: {protocol}</p>
               <p>Justificativa: {request.Justification}</p>
             </body>
             </html>
             """;

        return Task.FromResult(new FiscalNfeEventResult(
            "Inutilizacao registrada",
            protocol,
            AdapterName,
            xml.ToString(SaveOptions.DisableFormatting),
            preview));
    }

    public Task<FiscalNfeStatusResult> CheckStatusAsync(FiscalNfeStatusRequest request, CancellationToken cancellationToken)
        => Task.FromResult(new FiscalNfeStatusResult(
            AdapterName,
            "MockFiscalNfeEngine",
            true,
            false,
            false,
            null,
            "Simulado",
            "Adapter mock ativo apenas para desenvolvimento local. Nenhuma chamada real ao SEFAZ foi executada.",
            null,
            null));

    private static string BuildAccessKey(FiscalNfeEmissionRequest request)
    {
        var seed = $"{request.Emitter.DocumentNumber}{request.Emitter.FiscalSeries}{request.Emitter.NfeNumber}{request.Totals.InvoiceAmount:0.00}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return string.Concat(hash.Take(22).Select(x => (x % 10).ToString(CultureInfo.InvariantCulture)));
    }
}
