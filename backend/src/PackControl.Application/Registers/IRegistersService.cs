namespace PackControl.Application.Registers;

public interface IRegistersService
{
    Task<RegistersOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    Task<RegistersOverviewDto> CreateAsync(CreateRegisterEntryRequest request, CancellationToken cancellationToken);
    Task<RegistersOverviewDto> UpdateAsync(Guid registerEntryId, UpdateRegisterEntryRequest request, CancellationToken cancellationToken);
}
