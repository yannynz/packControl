using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Logistics;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/logistics")]
public sealed class LogisticsController(ILogisticsService logisticsService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken cancellationToken)
    {
        var overview = await logisticsService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpPost("shipments/{shipmentId:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(Guid shipmentId, CancellationToken cancellationToken)
    {
        var overview = await logisticsService.DispatchAsync(shipmentId, cancellationToken);
        return Ok(overview);
    }

    [HttpPost("shipments/{shipmentId:guid}/withdrawal")]
    public async Task<IActionResult> Withdrawal(Guid shipmentId, CancellationToken cancellationToken)
    {
        var overview = await logisticsService.MarkWithdrawalAsync(shipmentId, cancellationToken);
        return Ok(overview);
    }

    [HttpPost("shipments/{shipmentId:guid}/adverse")]
    public async Task<IActionResult> Adverse(Guid shipmentId, CancellationToken cancellationToken)
    {
        var overview = await logisticsService.MarkAdverseAsync(shipmentId, cancellationToken);
        return Ok(overview);
    }
}
