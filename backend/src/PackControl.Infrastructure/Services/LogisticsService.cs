using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Logistics;
using PackControl.Domain.Audit;
using PackControl.Domain.Orders;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class LogisticsService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor) : ILogisticsService
{
    public async Task<LogisticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return MapOverview(stateStore, clock.UtcNow);
        }
    }

    public async Task<LogisticsOverviewDto> DispatchAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return UpdateShipment(shipmentId, shipment =>
        {
            shipment.Status = "Em rota";
            shipment.ChecklistStatus = "Conferido";
            shipment.HasOccurrence = false;
        }, "logistics.dispatched", "Despacho confirmado");
    }

    public async Task<LogisticsOverviewDto> MarkWithdrawalAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return UpdateShipment(shipmentId, shipment =>
        {
            shipment.Mode = "Retirada";
            shipment.Status = "Retirada agendada";
            shipment.ChecklistStatus = "Aguardando retirada";
        }, "logistics.withdrawal", "Retirada configurada");
    }

    public async Task<LogisticsOverviewDto> MarkAdverseAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return UpdateShipment(shipmentId, shipment =>
        {
            shipment.Status = "Saida adversa";
            shipment.HasOccurrence = true;
            shipment.ChecklistStatus = "Ocorrencia aberta";
        }, "logistics.adverse", "Ocorrencia adversa registrada");
    }

    private LogisticsOverviewDto UpdateShipment(Guid shipmentId, Action<ShipmentState> apply, string eventType, string message)
    {
        lock (stateStore.SyncRoot)
        {
            var shipment = stateStore.Shipments.SingleOrDefault(x => x.Id == shipmentId);
            if (shipment is not null)
            {
                apply(shipment);
                var order = stateStore.Orders.SingleOrDefault(x => x.Id == shipment.OrderId);

                if (order is not null)
                {
                    stateStore.AuditLogs.Add(AuditLog.Create(
                        currentUserAccessor.UserId,
                        currentUserAccessor.DisplayName,
                        nameof(Order),
                        order.Id,
                        eventType,
                        $"{message} no lote {shipment.ShipmentNumber}.",
                        JsonSerializer.Serialize(new { shipment.Status, shipment.Mode }),
                        clock.UtcNow));
                }
            }

            return MapOverview(stateStore, clock.UtcNow);
        }
    }

    private static LogisticsOverviewDto MapOverview(AppStateStore stateStore, DateTime utcNow)
    {
        var shipments = stateStore.Shipments
            .OrderBy(x => x.ScheduledAtUtc)
            .Select(x => new ShipmentDto(
                x.Id,
                x.OrderId,
                x.OrderNumber,
                x.ShipmentNumber,
                x.CustomerName,
                x.Mode,
                x.Status,
                x.Recipient,
                x.CarrierId,
                x.CarrierName,
                x.DriverName,
                x.VehiclePlate,
                x.ChecklistStatus,
                x.HasOccurrence,
                x.ScheduledAtUtc))
            .ToList();

        return new LogisticsOverviewDto(
            shipments.Count(x => x.Status is "Aguardando producao" or "Pronto para expedir" or "Retirada agendada"),
            shipments.Count(x => x.ScheduledAtUtc.Date == utcNow.Date),
            shipments.Count(x => x.HasOccurrence),
            shipments);
    }
}
