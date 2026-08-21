using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Registers;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/registers")]
public sealed class RegistersController(IRegistersService registersService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var overview = await registersService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegisterEntryRequest request, CancellationToken cancellationToken)
    {
        var overview = await registersService.CreateAsync(request, cancellationToken);
        return Ok(overview);
    }

    [HttpPut("{registerEntryId:guid}")]
    public async Task<IActionResult> Update(Guid registerEntryId, [FromBody] UpdateRegisterEntryRequest request, CancellationToken cancellationToken)
    {
        var overview = await registersService.UpdateAsync(registerEntryId, request, cancellationToken);
        return Ok(overview);
    }
}
