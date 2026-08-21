namespace PackControl.Application.Carriers;

public interface ICarrierService
{
    Task<IReadOnlyList<CarrierDto>> ListAsync(CancellationToken cancellationToken);
    Task<CarrierDto> CreateAsync(CreateCarrierRequest request, CancellationToken cancellationToken);
    Task<CarrierDto?> UpdateAsync(Guid carrierId, UpdateCarrierRequest request, CancellationToken cancellationToken);
}
