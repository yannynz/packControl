namespace PackControl.Application.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
    Task<CustomerDto?> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken);
}
