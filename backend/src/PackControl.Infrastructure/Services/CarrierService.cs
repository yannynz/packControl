using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Carriers;
using PackControl.Domain.Audit;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class CarrierService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IAppStatePersistence statePersistence) : ICarrierService
{
    public async Task<IReadOnlyList<CarrierDto>> ListAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return stateStore.Carriers
                .OrderBy(x => x.Name)
                .Select(Map)
                .ToList();
        }
    }

    public async Task<CarrierDto> CreateAsync(CreateCarrierRequest request, CancellationToken cancellationToken)
    {
        CarrierDto carrierDto;
        lock (stateStore.SyncRoot)
        {
            var carrier = new CarrierState
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                ContactName = request.ContactName.Trim(),
                Email = request.Email.Trim(),
                Phone = request.Phone.Trim(),
                BusinessHours = request.BusinessHours.Trim(),
                ServiceArea = request.ServiceArea.Trim(),
                DefaultMode = request.DefaultMode.Trim(),
                DoesPickup = request.DoesPickup,
                DoesDelivery = request.DoesDelivery,
                Notes = request.Notes.Trim(),
                UpdatedAtUtc = clock.UtcNow
            };

            stateStore.Carriers.Add(carrier);
            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(CarrierState),
                carrier.Id,
                "carrier.created",
                $"Transportadora {carrier.Name} criada.",
                JsonSerializer.Serialize(new { carrier.DefaultMode, carrier.DoesDelivery }),
                clock.UtcNow));

            carrierDto = Map(carrier);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return carrierDto;
    }

    public async Task<CarrierDto?> UpdateAsync(Guid carrierId, UpdateCarrierRequest request, CancellationToken cancellationToken)
    {
        CarrierDto? carrierDto;
        lock (stateStore.SyncRoot)
        {
            var carrier = stateStore.Carriers.SingleOrDefault(x => x.Id == carrierId);
            if (carrier is null)
            {
                return null;
            }

            carrier.Name = request.Name.Trim();
            carrier.ContactName = request.ContactName.Trim();
            carrier.Email = request.Email.Trim();
            carrier.Phone = request.Phone.Trim();
            carrier.BusinessHours = request.BusinessHours.Trim();
            carrier.ServiceArea = request.ServiceArea.Trim();
            carrier.DefaultMode = request.DefaultMode.Trim();
            carrier.DoesPickup = request.DoesPickup;
            carrier.DoesDelivery = request.DoesDelivery;
            carrier.Notes = request.Notes.Trim();
            carrier.UpdatedAtUtc = clock.UtcNow;

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(CarrierState),
                carrier.Id,
                "carrier.updated",
                $"Transportadora {carrier.Name} atualizada.",
                JsonSerializer.Serialize(new { carrier.DefaultMode, carrier.DoesDelivery }),
                clock.UtcNow));

            carrierDto = Map(carrier);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return carrierDto;
    }

    private static CarrierDto Map(CarrierState carrier) => new(
        carrier.Id,
        carrier.Name,
        carrier.ContactName,
        carrier.Email,
        carrier.Phone,
        carrier.BusinessHours,
        carrier.ServiceArea,
        carrier.DefaultMode,
        carrier.DoesPickup,
        carrier.DoesDelivery,
        carrier.Notes,
        carrier.UpdatedAtUtc);
}
