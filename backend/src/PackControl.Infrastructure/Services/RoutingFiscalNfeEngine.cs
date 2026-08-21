using PackControl.Application.Fiscal;

namespace PackControl.Infrastructure.Services;

public sealed class RoutingFiscalNfeEngine(IEnumerable<IFiscalNfeEngineAdapter> adapters) : IFiscalNfeEngine
{
    private readonly IReadOnlyList<IFiscalNfeEngineAdapter> adapters = adapters.ToList();

    public Task<FiscalNfeEmissionResult> IssueAsync(FiscalNfeEmissionRequest request, CancellationToken cancellationToken)
        => ResolveAdapter(request.Emitter.AdapterName).IssueAsync(request, cancellationToken);

    public Task<FiscalNfeEventResult> CancelAsync(FiscalNfeCancellationRequest request, CancellationToken cancellationToken)
        => ResolveAdapter(request.Emitter.AdapterName).CancelAsync(request, cancellationToken);

    public Task<FiscalNfeEventResult> CorrectAsync(FiscalNfeCorrectionLetterRequest request, CancellationToken cancellationToken)
        => ResolveAdapter(request.Emitter.AdapterName).CorrectAsync(request, cancellationToken);

    public Task<FiscalNfeEventResult> InutilizeAsync(FiscalNfeInutilizationRequest request, CancellationToken cancellationToken)
        => ResolveAdapter(request.Emitter.AdapterName).InutilizeAsync(request, cancellationToken);

    public Task<FiscalNfeStatusResult> CheckStatusAsync(FiscalNfeStatusRequest request, CancellationToken cancellationToken)
        => ResolveAdapter(request.AdapterName).CheckStatusAsync(request, cancellationToken);

    private IFiscalNfeEngineAdapter ResolveAdapter(string adapterName)
    {
        var normalized = Normalize(adapterName);
        var adapter = adapters.SingleOrDefault(x => Normalize(x.AdapterName) == normalized);

        if (adapter is not null)
        {
            return adapter;
        }

        var supported = string.Join(", ", adapters.Select(x => x.AdapterName).OrderBy(x => x));
        throw new InvalidOperationException(
            $"Adapter fiscal '{adapterName}' nao e suportado. Adaptadores disponiveis: {supported}.");
    }

    private static string Normalize(string? value) => value?.Trim().ToLowerInvariant() ?? string.Empty;
}
