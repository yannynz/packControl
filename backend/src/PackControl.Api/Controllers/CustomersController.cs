using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Customers;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public sealed class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var customers = await customerService.ListAsync(cancellationToken);
        return Ok(customers);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { customer.Id }, customer);
    }

    [HttpPut("{customerId:guid}")]
    public async Task<IActionResult> Update(Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerService.UpdateAsync(customerId, request, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }
}
