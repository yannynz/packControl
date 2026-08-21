using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Customers;
using PackControl.Domain.Audit;
using PackControl.Domain.Customers;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class CustomerService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IAppStatePersistence statePersistence) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return stateStore.Customers
                .OrderBy(x => x.Name)
                .Select(Map)
                .ToList();
        }
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var actor = currentUserAccessor.DisplayName;
        CustomerDto customerDto;
        var customer = Customer.Create(
            request.Name,
            request.DocumentNumber,
            request.ContactName,
            request.Email,
            request.Phone,
            request.Notes,
            request.Nicknames,
            request.PostalCode,
            request.Street,
            request.StreetNumber,
            request.District,
            request.City,
            request.State,
            request.CityIbgeCode,
            request.StateRegistration,
            request.TaxpayerIndicator,
            request.Complement,
            request.ReferencePoint,
            request.DefaultCarrierId,
            request.DefaultCarrierName,
            request.DefaultDeliveryMode,
            request.ProductPricingRules.Select(MapPricingRule).ToList(),
            request.Score,
            clock.UtcNow,
            actor);

        lock (stateStore.SyncRoot)
        {
            stateStore.Customers.Add(customer);
            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                actor,
                nameof(Customer),
                customer.Id,
                "customer.created",
                $"Cliente {customer.Name} criado",
                JsonSerializer.Serialize(new { customer.Email, customer.Phone }),
                clock.UtcNow));

            customerDto = Map(customer);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return customerDto;
    }

    public async Task<CustomerDto?> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var actor = currentUserAccessor.DisplayName;
        CustomerDto? customerDto;

        lock (stateStore.SyncRoot)
        {
            var customer = stateStore.Customers.SingleOrDefault(x => x.Id == customerId);
            if (customer is null)
            {
                return null;
            }

            customer.Update(
                request.Name,
                request.DocumentNumber,
                request.ContactName,
                request.Email,
                request.Phone,
                request.Notes,
                request.Nicknames,
                request.PostalCode,
                request.Street,
                request.StreetNumber,
                request.District,
                request.City,
                request.State,
                request.CityIbgeCode,
                request.StateRegistration,
                request.TaxpayerIndicator,
                request.Complement,
                request.ReferencePoint,
                request.DefaultCarrierId,
                request.DefaultCarrierName,
                request.DefaultDeliveryMode,
                request.ProductPricingRules.Select(MapPricingRule).ToList(),
                request.Score,
                clock.UtcNow,
                actor);

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                actor,
                nameof(Customer),
                customer.Id,
                "customer.updated",
                $"Cliente {customer.Name} atualizado",
                JsonSerializer.Serialize(new { customer.DefaultCarrierName, customer.City, customer.State }),
                clock.UtcNow));

            customerDto = Map(customer);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return customerDto;
    }

    private static CustomerDto Map(Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.DocumentNumber,
        customer.ContactName,
        customer.Email,
        customer.Phone,
        customer.Notes,
        customer.Nicknames,
        customer.PostalCode,
        customer.Street,
        customer.StreetNumber,
        customer.District,
        customer.City,
        customer.State,
        customer.CityIbgeCode,
        customer.StateRegistration,
        customer.TaxpayerIndicator,
        customer.Complement,
        customer.ReferencePoint,
        customer.DefaultCarrierId,
        customer.DefaultCarrierName,
        customer.DefaultDeliveryMode,
        customer.ProductPricingRules
            .Select(x => new CustomerProductPricingRuleDto(x.ProductTemplateId, x.ProductName, x.BillingMethod, x.UnitPrice, x.Notes))
            .ToList(),
        customer.Score);

    private static CustomerProductPricingRule MapPricingRule(CustomerProductPricingRuleRequest request) =>
        CustomerProductPricingRule.Create(
            request.ProductTemplateId,
            request.ProductName,
            request.BillingMethod,
            request.UnitPrice,
            request.Notes);
}
