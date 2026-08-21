using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Finance;
using PackControl.Application.Fiscal;
using PackControl.Domain.Audit;
using PackControl.Domain.Orders;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class FinanceService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IAppStatePersistence statePersistence,
    IFiscalDocumentService fiscalDocumentService) : IFinanceService
{
    public async Task<FinanceOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return MapOverview(stateStore, clock.UtcNow);
        }
    }

    public async Task<FinanceOverviewDto> CreateEntryAsync(CreateFinanceEntryRequest request, CancellationToken cancellationToken)
    {
        FinanceOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            Order? order = null;
            if (request.OrderId is not null)
            {
                order = stateStore.Orders.SingleOrDefault(x => x.Id == request.OrderId.Value);
                if (order is null)
                {
                    throw new InvalidOperationException("Pedido vinculado nao encontrado.");
                }
            }

            var entry = new FinanceEntryState
            {
                Id = Guid.NewGuid(),
                OrderId = order?.Id,
                OrderNumber = order?.Number ?? Normalize(request.OrderNumber),
                Type = request.Type.Trim(),
                Status = request.Type == "Receber" ? "Em aberto" : "Programado",
                Description = request.Description.Trim(),
                Counterparty = request.Counterparty.Trim(),
                Amount = decimal.Round(Math.Max(0, request.Amount), 2),
                DueAtUtc = request.DueAtUtc,
                PaymentMethod = request.PaymentMethod.Trim(),
                Notes = Normalize(request.Notes),
                EntrySource = request.EntrySource.Trim()
            };

            stateStore.FinanceEntries.Add(entry);

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                order is null ? nameof(FinanceEntryState) : nameof(Order),
                order?.Id ?? entry.Id,
                "finance.entry_created",
                $"Lancamento de {entry.Type.ToLowerInvariant()} criado no financeiro.",
                JsonSerializer.Serialize(new { entry.Amount, entry.PaymentMethod, entry.EntrySource }),
                clock.UtcNow));

            overview = MapOverview(stateStore, clock.UtcNow);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    public async Task<FinanceOverviewDto> SettleAsync(Guid entryId, CancellationToken cancellationToken)
    {
        FinanceOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            var entry = stateStore.FinanceEntries.SingleOrDefault(x => x.Id == entryId);
            if (entry is not null)
            {
                entry.Status = "Liquidado";
                var order = entry.OrderId is null ? null : stateStore.Orders.SingleOrDefault(x => x.Id == entry.OrderId.Value);

                if (order is not null)
                {
                    stateStore.AuditLogs.Add(AuditLog.Create(
                        currentUserAccessor.UserId,
                        currentUserAccessor.DisplayName,
                        nameof(Order),
                        order.Id,
                        "finance.entry_settled",
                        $"Lancamento financeiro de {entry.Type.ToLowerInvariant()} liquidado.",
                        JsonSerializer.Serialize(new { entry.Type, entry.Amount }),
                        clock.UtcNow));
                }
            }

            overview = MapOverview(stateStore, clock.UtcNow);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    public async Task<FinanceOverviewDto> GenerateBoletoAsync(Guid entryId, CancellationToken cancellationToken)
    {
        FinanceOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            var entry = stateStore.FinanceEntries.SingleOrDefault(x => x.Id == entryId);
            if (entry is not null && entry.Type == "Receber")
            {
                entry.PaymentMethod = "Boleto";
                entry.BoletoStatus = "Emitido";
                entry.BoletoNumber = $"2379{clock.UtcNow:yyMMdd}{stateStore.FinanceEntries.Count(x => x.BoletoNumber is not null):000000}";
                entry.BoletoLine = $"23790.{clock.UtcNow:MMdd} {entry.Amount:00000000} {entry.Id.ToString("N")[..10]}";

                var order = entry.OrderId is null ? null : stateStore.Orders.SingleOrDefault(x => x.Id == entry.OrderId.Value);
                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    order is null ? nameof(FinanceEntryState) : nameof(Order),
                    order?.Id ?? entry.Id,
                    "finance.boleto_generated",
                    $"Boleto gerado para {entry.Counterparty}.",
                    JsonSerializer.Serialize(new { entry.BoletoNumber, entry.Amount }),
                    clock.UtcNow));
            }

            overview = MapOverview(stateStore, clock.UtcNow);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    public async Task<FinanceOverviewDto> IssueInvoiceAsync(IssueFiscalInvoiceRequest request, CancellationToken cancellationToken)
    {
        await fiscalDocumentService.IssueAsync(
            new IssueFiscalDocumentCommand(
                null,
                request.FinanceEntryId,
                null,
                Normalize(request.Series),
                Normalize(request.NatureOfOperation),
                Normalize(request.Cfop),
                Normalize(request.Notes)),
            cancellationToken);

        lock (stateStore.SyncRoot)
        {
            return MapOverview(stateStore, clock.UtcNow);
        }
    }

    private static FinanceOverviewDto MapOverview(AppStateStore stateStore, DateTime utcNow)
    {
        var entries = stateStore.FinanceEntries
            .OrderBy(x => x.DueAtUtc)
            .Select(x => new FinanceEntryDto(
                x.Id,
                x.OrderId,
                x.OrderNumber,
                x.Type,
                x.Status,
                x.Description,
                x.Counterparty,
                x.Amount,
                x.EntrySource,
                x.PaymentMethod,
                x.Notes,
                x.BoletoStatus,
                x.BoletoNumber,
                x.BoletoLine,
                x.DueAtUtc))
            .ToList();

        var invoices = stateStore.FiscalInvoices
            .OrderByDescending(x => x.IssuedAtUtc)
            .Select(x => new FiscalInvoiceDto(
                x.Id,
                x.FinanceEntryId,
                x.OrderId,
                x.OrderNumber,
                x.Number,
                x.Series,
                x.Environment,
                x.AccessKey,
                x.Protocol,
                x.EngineName,
                x.CertificateType,
                x.CertificateMedia,
                x.NatureOfOperation,
                x.Cfop,
                x.XmlArchivePath,
                x.DanfeArchivePath,
                x.CustomerName,
                x.Status,
                x.Amount,
                x.IssuedAtUtc,
                x.Notes))
            .ToList();

        return new FinanceOverviewDto(
            entries.Where(x => x.Type == "Receber" && x.Status != "Liquidado").Sum(x => x.Amount),
            entries.Where(x => x.Type == "Pagar" && x.Status != "Liquidado").Sum(x => x.Amount),
            entries.Where(x => x.Status != "Liquidado" && x.DueAtUtc.Date < utcNow.Date).Sum(x => x.Amount),
            entries,
            invoices);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
