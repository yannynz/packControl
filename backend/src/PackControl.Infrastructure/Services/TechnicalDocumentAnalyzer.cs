using System.Text.RegularExpressions;
using IxMilia.Dxf;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace PackControl.Infrastructure.Services;

public sealed record TechnicalDocumentAnalysisResult(
    bool Success,
    string Summary,
    string EngineName,
    int ConfidencePercent);

public sealed class TechnicalDocumentAnalyzer
{
    public async Task<TechnicalDocumentAnalysisResult> AnalyzeAsync(
        string fileExtension,
        Stream source,
        CancellationToken cancellationToken)
    {
        var normalizedExtension = fileExtension.Trim().ToLowerInvariant();
        return normalizedExtension switch
        {
            ".pdf" => await AnalyzePdfAsync(source, cancellationToken),
            ".dxf" => await AnalyzeDxfAsync(source, cancellationToken),
            _ => new TechnicalDocumentAnalysisResult(false, "Formato sem analisador tecnico configurado.", "PackControl", 0)
        };
    }

    private static async Task<TechnicalDocumentAnalysisResult> AnalyzePdfAsync(Stream source, CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        using var document = PdfDocument.Open(buffer);
        var pages = document.GetPages().ToList();
        var texts = pages
            .Select(page => ContentOrderTextExtractor.GetText(page))
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();

        var combinedText = Regex.Replace(string.Join(" ", texts), @"\s+", " ").Trim();
        var dimensions = Regex.Matches(combinedText, @"\b\d{2,5}\s*[xX]\s*\d{2,5}\s*(mm|cm|m)?\b")
            .Select(match => match.Value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();
        var quantityMatch = Regex.Match(combinedText, @"\b(qtd|quantidade)\s*[:\-]?\s*(\d+)\b", RegexOptions.IgnoreCase);
        var extractedQuantity = quantityMatch.Success ? quantityMatch.Groups[2].Value : null;

        var summaryParts = new List<string>
        {
            $"PDF lido com {pages.Count} pagina(s)",
            $"{combinedText.Length} caractere(s) uteis"
        };

        if (!string.IsNullOrWhiteSpace(extractedQuantity))
        {
            summaryParts.Add($"quantidade detectada {extractedQuantity}");
        }

        if (dimensions.Count > 0)
        {
            summaryParts.Add($"medidas detectadas {string.Join(", ", dimensions)}");
        }

        var confidence = 55;
        if (pages.Count > 0)
        {
            confidence += 15;
        }

        if (combinedText.Length > 80)
        {
            confidence += 15;
        }

        if (dimensions.Count > 0)
        {
            confidence += 15;
        }

        return new TechnicalDocumentAnalysisResult(true, string.Join("; ", summaryParts) + ".", "PdfPig", Math.Clamp(confidence, 0, 100));
    }

    private static async Task<TechnicalDocumentAnalysisResult> AnalyzeDxfAsync(Stream source, CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var dxf = DxfFile.Load(buffer);
        var entities = dxf.Entities.ToList();
        var layers = entities
            .Select(entity => entity.Layer)
            .Where(layer => !string.IsNullOrWhiteSpace(layer))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var entityMix = entities
            .GroupBy(entity => entity.GetType().Name.Replace("Dxf", string.Empty))
            .OrderByDescending(group => group.Count())
            .Take(4)
            .Select(group => $"{group.Key}:{group.Count()}")
            .ToList();

        var summary = $"DXF lido com {entities.Count} entidade(s), {layers.Count} layer(s) e mix {string.Join(", ", entityMix)}.";
        var confidence = Math.Clamp(45 + (entities.Count > 0 ? 25 : 0) + Math.Min(layers.Count * 5, 20) + Math.Min(entityMix.Count * 5, 10), 0, 100);

        return new TechnicalDocumentAnalysisResult(true, summary, "IxMilia.Dxf", confidence);
    }
}
