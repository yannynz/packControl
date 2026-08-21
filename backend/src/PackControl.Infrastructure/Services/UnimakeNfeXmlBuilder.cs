using System.Globalization;
using PackControl.Application.Fiscal;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFe;

namespace PackControl.Infrastructure.Services;

internal static class UnimakeNfeXmlBuilder
{
    private const string Brasil = "Brasil";
    private const int CodigoPaisBrasil = 1058;

    public static EnviNFe Build(FiscalNfeEmissionRequest request, string schemaVersion, string processVersion)
    {
        ValidateRequest(request);

        var emitterUf = MapStateCode(request.Emitter.Address.StateCode);
        var recipientUf = MapStateCode(request.Recipient.Address.StateCode);
        var now = GetBrazilNow();
        var ide = new Ide
        {
            CUF = emitterUf,
            CNF = Random.Shared.Next(10000000, 99999999).ToString("D8", CultureInfo.InvariantCulture),
            NatOp = request.NatureOfOperation.Trim(),
            Mod = ModeloDFe.NFe,
            Serie = ParsePositiveInteger(request.Emitter.FiscalSeries, "serie fiscal"),
            NNF = request.Emitter.NfeNumber,
            DhEmi = now,
            DhSaiEnt = now,
            TpNF = TipoOperacao.Saida,
            IdDest = emitterUf == recipientUf
                ? DestinoOperacao.OperacaoInterna
                : DestinoOperacao.OperacaoInterestadual,
            CMunFG = ParseMunicipalityCode(request.Emitter.Address.CityIbgeCode, "emitente"),
            TpImp = FormatoImpressaoDANFE.NormalRetrato,
            TpEmis = TipoEmissao.Normal,
            TpAmb = MapEnvironment(request.Emitter.Environment),
            FinNFe = FinalidadeNFe.Normal,
            IndFinal = MapConsumerFinal(request.Recipient.TaxpayerIndicator),
            IndPres = IndicadorPresenca.NaoSeAplica,
            ProcEmi = ProcessoEmissao.AplicativoContribuinte,
            VerProc = processVersion
        };

        var infNFe = new InfNFe
        {
            Versao = schemaVersion,
            Ide = ide,
            Emit = BuildEmitter(request.Emitter),
            Dest = BuildRecipient(request.Recipient),
            Det = request.Items.Select(BuildItem).ToList(),
            Total = BuildTotal(request),
            Transp = BuildTransport(request.Transport),
            Pag = BuildPayment(request.Payment, now),
            InfAdic = BuildAdditionalInfo(request)
        };

        var cobr = BuildBilling(request.Payment, request.Emitter.NfeNumber);
        if (cobr is not null)
        {
            infNFe.Cobr = cobr;
        }

        return new EnviNFe
        {
            Versao = schemaVersion,
            IdLote = now.ToString("yyMMddHHmmssfff", CultureInfo.InvariantCulture),
            IndSinc = SimNao.Sim,
            NFe =
            [
                new NFe
                {
                    InfNFe = [infNFe]
                }
            ]
        };
    }

    private static Emit BuildEmitter(FiscalEmitterProfile emitter)
    {
        var document = ParseDocument(emitter.DocumentNumber, "emitente");

        return new Emit
        {
            CNPJ = document.IsCnpj ? document.Digits : null,
            CPF = document.IsCpf ? document.Digits : null,
            XNome = emitter.TradeName.Trim(),
            XFant = emitter.TradeName.Trim(),
            IE = NormalizeRequiredText(emitter.StateRegistration, "IE do emitente"),
            CRT = MapTaxRegime(emitter.TaxRegime),
            EnderEmit = new EnderEmit
            {
                XLgr = NormalizeRequiredText(emitter.Address.Street, "logradouro do emitente"),
                Nro = NormalizeRequiredText(emitter.Address.StreetNumber, "numero do emitente"),
                XCpl = NormalizeOptionalText(emitter.Address.Complement),
                XBairro = NormalizeRequiredText(emitter.Address.District, "bairro do emitente"),
                CMun = ParseMunicipalityCode(emitter.Address.CityIbgeCode, "emitente"),
                XMun = NormalizeRequiredText(emitter.Address.City, "cidade do emitente"),
                UF = MapStateCode(emitter.Address.StateCode),
                CEP = DigitsOnly(emitter.Address.PostalCode),
                CPais = CodigoPaisBrasil,
                XPais = Brasil
            }
        };
    }

    private static Dest BuildRecipient(FiscalRecipientProfile recipient)
    {
        var document = ParseDocument(recipient.DocumentNumber, "destinatario");
        var taxpayerIndicator = MapTaxpayerIndicator(recipient.TaxpayerIndicator);

        return new Dest
        {
            CNPJ = document.IsCnpj ? document.Digits : null,
            CPF = document.IsCpf ? document.Digits : null,
            XNome = NormalizeRequiredText(recipient.Name, "nome do destinatario"),
            Email = NormalizeOptionalText(recipient.Email),
            IE = taxpayerIndicator == IndicadorIEDestinatario.ContribuinteICMS
                ? NormalizeRequiredText(recipient.StateRegistration, "IE do destinatario contribuinte")
                : null,
            IndIEDest = taxpayerIndicator,
            EnderDest = new EnderDest
            {
                XLgr = NormalizeRequiredText(recipient.Address.Street, "logradouro do destinatario"),
                Nro = NormalizeRequiredText(recipient.Address.StreetNumber, "numero do destinatario"),
                XCpl = NormalizeOptionalText(recipient.Address.Complement),
                XBairro = NormalizeRequiredText(recipient.Address.District, "bairro do destinatario"),
                CMun = ParseMunicipalityCode(recipient.Address.CityIbgeCode, "destinatario"),
                XMun = NormalizeRequiredText(recipient.Address.City, "cidade do destinatario"),
                UF = MapStateCode(recipient.Address.StateCode),
                CEP = DigitsOnly(recipient.Address.PostalCode),
                CPais = CodigoPaisBrasil,
                XPais = Brasil,
                Fone = NormalizeOptionalPhone(recipient.Phone)
            }
        };
    }

    private static Det BuildItem(FiscalNfeItem item)
    {
        var icms = BuildIcms(item);
        var imposto = new Imposto
        {
            ICMS = icms,
            IPI = BuildIpi(item),
            PIS = BuildPis(item),
            COFINS = BuildCofins(item),
            VTotTrib = RoundDouble(item.IcmsAmount + item.IpiAmount + item.PisAmount + item.CofinsAmount)
        };

        return new Det
        {
            NItem = item.LineNumber,
            Prod = new Prod
            {
                CProd = BuildProductCode(item),
                CEAN = "SEM GTIN",
                CEANTrib = "SEM GTIN",
                XProd = NormalizeRequiredText(item.Description, $"descricao do item {item.LineNumber}"),
                NCM = NormalizeNcm(item.Ncm, item.LineNumber),
                CFOP = NormalizeCfop(item.Cfop, item.LineNumber),
                UCom = NormalizeRequiredText(item.CommercialUnit, $"unidade comercial do item {item.LineNumber}"),
                QCom = RoundDecimal(item.Quantity),
                VUnCom = RoundDecimal(item.UnitPrice),
                VProd = RoundDouble(item.GrossAmount),
                CBarra = null,
                CBarraTrib = null,
                UTrib = NormalizeRequiredText(item.CommercialUnit, $"unidade tributavel do item {item.LineNumber}"),
                QTrib = RoundDecimal(item.TaxQuantity),
                VUnTrib = RoundDecimal(item.UnitPrice),
                VDesc = RoundDouble(item.DiscountAmount),
                IndTot = SimNao.Sim
            },
            Imposto = imposto,
            InfAdProd = NormalizeOptionalText(item.Notes),
            VItem = RoundDouble(item.TotalAmount)
        };
    }

    private static ICMS BuildIcms(FiscalNfeItem item)
    {
        var cst = NormalizeRequiredText(item.IcmsSituationCode, $"CST ICMS do item {item.LineNumber}");
        var origin = MapOrigin(item.OriginCode);

        return cst switch
        {
            "00" => new ICMS
            {
                ICMS00 = new ICMS00
                {
                    Orig = origin,
                    CST = cst,
                    ModBC = ModalidadeBaseCalculoICMS.ValorOperacao,
                    VBC = RoundDouble(item.IcmsBaseAmount),
                    PICMS = RoundDouble(item.IcmsRate),
                    VICMS = RoundDouble(item.IcmsAmount)
                }
            },
            "40" or "41" or "50" => new ICMS
            {
                ICMS40 = new ICMS40
                {
                    Orig = origin,
                    CST = cst
                }
            },
            "90" => new ICMS
            {
                ICMS90 = new ICMS90
                {
                    Orig = origin,
                    CST = cst,
                    ModBC = ModalidadeBaseCalculoICMS.ValorOperacao,
                    VBC = RoundDouble(item.IcmsBaseAmount),
                    PICMS = RoundDouble(item.IcmsRate),
                    VICMS = RoundDouble(item.IcmsAmount)
                }
            },
            _ => throw new InvalidOperationException(
                $"CST ICMS '{cst}' do item {item.LineNumber} ainda nao esta mapeado na trilha real Unimake.")
        };
    }

    private static IPI BuildIpi(FiscalNfeItem item)
        => new()
        {
            CEnq = "999",
            IPITrib = item.IpiAmount > 0m || item.IpiRate > 0m
                ? new IPITrib
                {
                    CST = NormalizeRequiredText(item.IpiSituationCode, $"CST IPI do item {item.LineNumber}"),
                    VBC = RoundDouble(item.TotalAmount),
                    PIPI = RoundDouble(item.IpiRate),
                    VIPI = RoundDouble(item.IpiAmount)
                }
                : null,
            IPINT = item.IpiAmount > 0m || item.IpiRate > 0m
                ? null
                : new IPINT
                {
                    CST = NormalizeRequiredText(item.IpiSituationCode, $"CST IPI do item {item.LineNumber}")
                }
        };

    private static PIS BuildPis(FiscalNfeItem item)
        => IsAliquotaTax(item.PisAmount, item.PisRate, item.PisSituationCode)
            ? new PIS
            {
                PISAliq = new PISAliq
                {
                    CST = NormalizeRequiredText(item.PisSituationCode, $"CST PIS do item {item.LineNumber}"),
                    VBC = RoundDouble(item.TotalAmount),
                    PPIS = RoundDouble(item.PisRate),
                    VPIS = RoundDouble(item.PisAmount)
                }
            }
            : new PIS
            {
                PISNT = new PISNT
                {
                    CST = NormalizeRequiredText(item.PisSituationCode, $"CST PIS do item {item.LineNumber}")
                }
            };

    private static COFINS BuildCofins(FiscalNfeItem item)
        => IsAliquotaTax(item.CofinsAmount, item.CofinsRate, item.CofinsSituationCode)
            ? new COFINS
            {
                COFINSAliq = new COFINSAliq
                {
                    CST = NormalizeRequiredText(item.CofinsSituationCode, $"CST COFINS do item {item.LineNumber}"),
                    VBC = RoundDouble(item.TotalAmount),
                    PCOFINS = RoundDouble(item.CofinsRate),
                    VCOFINS = RoundDouble(item.CofinsAmount)
                }
            }
            : new COFINS
            {
                COFINSNT = new COFINSNT
                {
                    CST = NormalizeRequiredText(item.CofinsSituationCode, $"CST COFINS do item {item.LineNumber}")
                }
            };

    private static Total BuildTotal(FiscalNfeEmissionRequest request)
        => new()
        {
            ICMSTot = new ICMSTot
            {
                VBC = RoundDouble(request.Totals.IcmsBaseAmount),
                VICMS = RoundDouble(request.Totals.IcmsAmount),
                VProd = RoundDouble(request.Totals.ProductsAmount),
                VFrete = RoundDouble(request.Totals.FreightAmount),
                VSeg = RoundDouble(request.Totals.InsuranceAmount),
                VDesc = RoundDouble(request.Totals.DiscountAmount),
                VIPI = RoundDouble(request.Totals.IpiAmount),
                VPIS = RoundDouble(request.Totals.PisAmount),
                VCOFINS = RoundDouble(request.Totals.CofinsAmount),
                VOutro = RoundDouble(request.Totals.OtherAmount),
                VNF = RoundDouble(request.Totals.InvoiceAmount),
                VTotTrib = RoundDouble(
                    request.Totals.IcmsAmount +
                    request.Totals.IpiAmount +
                    request.Totals.PisAmount +
                    request.Totals.CofinsAmount)
            }
        };

    private static Transp BuildTransport(FiscalNfeTransport transport)
    {
        var transp = new Transp
        {
            ModFrete = MapFreightMode(transport.FreightMode)
        };

        if (!string.IsNullOrWhiteSpace(transport.CarrierName))
        {
            transp.Transporta = new Transporta
            {
                XNome = transport.CarrierName.Trim()
            };
        }

        if (!string.IsNullOrWhiteSpace(transport.VehiclePlate))
        {
            transp.VeicTransp = new VeicTransp
            {
                Placa = transport.VehiclePlate.Trim().ToUpperInvariant()
            };
        }

        return transp;
    }

    private static Pag BuildPayment(FiscalNfePayment payment, DateTimeOffset now)
        => new()
        {
            DetPag =
            [
                new DetPag
                {
                    IndPag = MapPaymentIndicator(payment.BillingType),
                    TPag = MapPaymentMethod(payment.PaymentMethod, payment.BillingType),
                    VPag = RoundDouble(payment.BillingAmount),
                    DPag = payment.DueAtUtc.HasValue
                        ? DateTime.SpecifyKind(payment.DueAtUtc.Value.Date, DateTimeKind.Unspecified)
                        : DateTime.SpecifyKind(now.Date, DateTimeKind.Unspecified),
                    XPag = NormalizeOptionalText(payment.PaymentMethod)
                }
            ]
        };

    private static Cobr? BuildBilling(FiscalNfePayment payment, int nfeNumber)
    {
        if (!payment.DueAtUtc.HasValue || payment.BillingAmount <= 0m)
        {
            return null;
        }

        return new Cobr
        {
            Fat = new Fat
            {
                NFat = $"FAT-{nfeNumber:D9}",
                VOrig = RoundDouble(payment.BillingAmount),
                VDesc = 0d,
                VLiq = RoundDouble(payment.BillingAmount)
            },
            Dup =
            [
                new Dup
                {
                    NDup = NormalizeOptionalText(payment.BoletoNumber) ?? $"DUP-{nfeNumber:D9}",
                    DVenc = DateTime.SpecifyKind(payment.DueAtUtc.Value.Date, DateTimeKind.Unspecified),
                    VDup = RoundDouble(payment.BillingAmount)
                }
            ]
        };
    }

    private static InfAdic? BuildAdditionalInfo(FiscalNfeEmissionRequest request)
    {
        var notes = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.AdditionalInformation))
        {
            notes.Add(request.AdditionalInformation.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Payment.BoletoNumber))
        {
            notes.Add($"Boleto: {request.Payment.BoletoNumber.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(request.Payment.BoletoLine))
        {
            notes.Add($"Linha digitavel: {request.Payment.BoletoLine.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(request.Transport.CarrierName))
        {
            notes.Add($"Transportadora: {request.Transport.CarrierName.Trim()}");
        }

        if (notes.Count == 0)
        {
            return null;
        }

        return new InfAdic
        {
            InfCpl = string.Join(" | ", notes)
        };
    }

    private static void ValidateRequest(FiscalNfeEmissionRequest request)
    {
        var issues = new List<string>();

        if (request.Emitter.NfeNumber <= 0)
        {
            issues.Add("Numero da NF-e deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(request.NatureOfOperation))
        {
            issues.Add("Natureza da operacao fiscal nao informada.");
        }

        if (string.IsNullOrWhiteSpace(request.Emitter.DocumentNumber))
        {
            issues.Add("Documento do emitente nao informado.");
        }

        if (string.IsNullOrWhiteSpace(request.Recipient.DocumentNumber))
        {
            issues.Add("Documento do destinatario nao informado.");
        }

        if (request.Items.Count == 0)
        {
            issues.Add("Documento fiscal precisa de pelo menos um item.");
        }

        if (!IsValidMunicipalityCode(request.Emitter.Address.CityIbgeCode))
        {
            issues.Add("Codigo IBGE do municipio do emitente deve ter 7 digitos.");
        }

        if (!IsValidMunicipalityCode(request.Recipient.Address.CityIbgeCode))
        {
            issues.Add("Codigo IBGE do municipio do destinatario deve ter 7 digitos.");
        }

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0m)
            {
                issues.Add($"Item {item.LineNumber} com quantidade invalida.");
            }

            if (item.UnitPrice < 0m || item.TotalAmount < 0m)
            {
                issues.Add($"Item {item.LineNumber} com valores negativos.");
            }

            if (DigitsOnly(item.Cfop).Length != 4)
            {
                issues.Add($"Item {item.LineNumber} com CFOP invalido.");
            }

            if (DigitsOnly(item.Ncm).Length != 8)
            {
                issues.Add($"Item {item.LineNumber} com NCM invalido.");
            }
        }

        if (issues.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" | ", issues.Distinct(StringComparer.OrdinalIgnoreCase)));
        }
    }

    private static TipoAmbiente MapEnvironment(string? environment)
        => string.Equals(environment?.Trim(), "Producao", StringComparison.OrdinalIgnoreCase)
            ? TipoAmbiente.Producao
            : TipoAmbiente.Homologacao;

    private static UFBrasil MapStateCode(string? stateCode)
    {
        var normalized = stateCode?.Trim().ToUpperInvariant();
        return normalized switch
        {
            "RO" => UFBrasil.RO,
            "AC" => UFBrasil.AC,
            "AM" => UFBrasil.AM,
            "RR" => UFBrasil.RR,
            "PA" => UFBrasil.PA,
            "AP" => UFBrasil.AP,
            "TO" => UFBrasil.TO,
            "MA" => UFBrasil.MA,
            "PI" => UFBrasil.PI,
            "CE" => UFBrasil.CE,
            "RN" => UFBrasil.RN,
            "PB" => UFBrasil.PB,
            "PE" => UFBrasil.PE,
            "AL" => UFBrasil.AL,
            "SE" => UFBrasil.SE,
            "BA" => UFBrasil.BA,
            "MG" => UFBrasil.MG,
            "ES" => UFBrasil.ES,
            "RJ" => UFBrasil.RJ,
            "SP" => UFBrasil.SP,
            "PR" => UFBrasil.PR,
            "SC" => UFBrasil.SC,
            "RS" => UFBrasil.RS,
            "MS" => UFBrasil.MS,
            "MT" => UFBrasil.MT,
            "GO" => UFBrasil.GO,
            "DF" => UFBrasil.DF,
            _ => throw new InvalidOperationException($"UF '{stateCode}' nao e suportada na emissao real.")
        };
    }

    private static CRT MapTaxRegime(string? taxRegime)
        => taxRegime?.Trim() switch
        {
            "Simples Nacional" => CRT.SimplesNacional,
            "MEI" => CRT.SimplesNacionalMEI,
            _ => CRT.RegimeNormal
        };

    private static IndicadorIEDestinatario MapTaxpayerIndicator(string? indicator)
        => indicator?.Trim() switch
        {
            "Contribuinte" => IndicadorIEDestinatario.ContribuinteICMS,
            "Isento" => IndicadorIEDestinatario.ContribuinteIsento,
            _ => IndicadorIEDestinatario.NaoContribuinte
        };

    private static SimNao MapConsumerFinal(string? indicator)
        => string.Equals(indicator?.Trim(), "NaoContribuinte", StringComparison.OrdinalIgnoreCase)
            ? SimNao.Sim
            : SimNao.Nao;

    private static OrigemMercadoria MapOrigin(string? originCode)
        => DigitsOnly(originCode) switch
        {
            "" or "0" => OrigemMercadoria.Nacional,
            "1" => OrigemMercadoria.Estrangeira,
            "2" => OrigemMercadoria.Estrangeira2,
            "3" => OrigemMercadoria.Nacional3,
            "4" => OrigemMercadoria.Nacional4,
            "5" => OrigemMercadoria.Nacional5,
            "6" => OrigemMercadoria.Estrangeira6,
            "7" => OrigemMercadoria.Estrangeira7,
            "8" => OrigemMercadoria.Nacional8,
            _ => throw new InvalidOperationException($"Origem fiscal '{originCode}' nao e suportada.")
        };

    private static ModalidadeFrete MapFreightMode(string? freightMode)
        => freightMode?.Trim() switch
        {
            "Emitente" => ModalidadeFrete.ContratacaoFretePorContaRemetente_CIF,
            "Destinatario" => ModalidadeFrete.ContratacaoFretePorContaDestinatário_FOB,
            "Terceiros" => ModalidadeFrete.ContratacaoFretePorContaTerceiros,
            _ => ModalidadeFrete.SemOcorrenciaTransporte
        };

    private static IndicadorPagamento MapPaymentIndicator(string? billingType)
        => string.Equals(billingType?.Trim(), "A prazo", StringComparison.OrdinalIgnoreCase)
            ? IndicadorPagamento.PagamentoPrazo
            : IndicadorPagamento.PagamentoVista;

    private static MeioPagamento MapPaymentMethod(string? paymentMethod, string? billingType)
    {
        var normalized = paymentMethod?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Equals(billingType?.Trim(), "A prazo", StringComparison.OrdinalIgnoreCase)
                ? MeioPagamento.PagamentoPosterior
                : MeioPagamento.Outros;
        }

        return normalized switch
        {
            var value when value.Contains("boleto", StringComparison.Ordinal)
                => MeioPagamento.BoletoBancario,
            var value when value.Contains("pix", StringComparison.Ordinal)
                => MeioPagamento.PagamentoInstantaneo,
            var value when value.Contains("transfer", StringComparison.Ordinal)
                => MeioPagamento.TransferenciaBancaria,
            var value when value.Contains("deposit", StringComparison.Ordinal)
                => MeioPagamento.DepositoBancario,
            var value when value.Contains("credito", StringComparison.Ordinal)
                => MeioPagamento.CartaoCredito,
            var value when value.Contains("debito", StringComparison.Ordinal)
                => MeioPagamento.CartaoDebito,
            var value when value.Contains("dinheiro", StringComparison.Ordinal)
                => MeioPagamento.Dinheiro,
            _ when string.Equals(billingType?.Trim(), "A prazo", StringComparison.OrdinalIgnoreCase)
                => MeioPagamento.PagamentoPosterior,
            _ => MeioPagamento.Outros
        };
    }

    private static bool IsAliquotaTax(decimal amount, decimal rate, string? situationCode)
        => amount > 0m ||
            rate > 0m ||
            situationCode?.Trim() is "01" or "02";

    private static string BuildProductCode(FiscalNfeItem item)
    {
        if (item.ProductTemplateId.HasValue)
        {
            return item.ProductTemplateId.Value.ToString("N")[..8].ToUpperInvariant();
        }

        return $"ITEM{item.LineNumber:D3}";
    }

    private static string NormalizeNcm(string? ncm, int lineNumber)
    {
        var digits = DigitsOnly(ncm);
        return digits.Length == 8
            ? digits
            : throw new InvalidOperationException($"NCM do item {lineNumber} deve ter 8 digitos.");
    }

    private static string NormalizeCfop(string? cfop, int lineNumber)
    {
        var digits = DigitsOnly(cfop);
        return digits.Length == 4
            ? digits
            : throw new InvalidOperationException($"CFOP do item {lineNumber} deve ter 4 digitos.");
    }

    private static int ParseMunicipalityCode(string? cityIbgeCode, string label)
    {
        var digits = DigitsOnly(cityIbgeCode);
        if (digits.Length != 7 || !int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var code))
        {
            throw new InvalidOperationException($"Codigo IBGE do municipio de {label} deve ter 7 digitos.");
        }

        return code;
    }

    private static int ParsePositiveInteger(string? value, string label)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            throw new InvalidOperationException($"Campo '{label}' deve ser numerico e maior que zero.");
        }

        return parsed;
    }

    private static (string Digits, bool IsCnpj, bool IsCpf) ParseDocument(string? documentNumber, string label)
    {
        var digits = DigitsOnly(documentNumber);
        return digits.Length switch
        {
            14 => (digits, true, false),
            11 => (digits, false, true),
            _ => throw new InvalidOperationException($"Documento de {label} deve ser CPF ou CNPJ valido.")
        };
    }

    private static bool IsValidMunicipalityCode(string? value) => DigitsOnly(value).Length == 7;

    private static string NormalizeRequiredText(string? value, string label)
        => string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Campo '{label}' e obrigatorio para emissao real.")
            : value.Trim();

    private static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeOptionalPhone(string? value)
    {
        var digits = DigitsOnly(value);
        return digits.Length == 0 ? null : digits;
    }

    private static string DigitsOnly(string? value) => new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static decimal RoundDecimal(decimal value) => decimal.Round(value, 4, MidpointRounding.AwayFromZero);

    private static double RoundDouble(decimal value) => Math.Round((double)value, 2, MidpointRounding.AwayFromZero);

    private static DateTimeOffset GetBrazilNow()
    {
        var utcNow = DateTimeOffset.UtcNow;

        try
        {
            return TimeZoneInfo.ConvertTime(utcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"));
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.ConvertTime(utcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        }
    }
}
