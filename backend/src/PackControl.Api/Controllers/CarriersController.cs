using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Carriers;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/carriers")]
public sealed class CarriersController(ICarrierService carrierService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var carriers = await carrierService.ListAsync(cancellationToken);
        return Ok(carriers);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCarrierRequest request, CancellationToken cancellationToken)
    {
        var carrier = await carrierService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { carrier.Id }, carrier);
    }

    [HttpPut("{carrierId:guid}")]
    public async Task<IActionResult> Update(
        Guid carrierId,
        [FromBody] UpdateCarrierRequest request,
        CancellationToken cancellationToken)
    {
        var carrier = await carrierService.UpdateAsync(carrierId, request, cancellationToken);
        return carrier is null ? NotFound() : Ok(carrier);
    }
}
