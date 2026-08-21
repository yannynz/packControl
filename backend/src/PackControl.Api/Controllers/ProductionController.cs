using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Production;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/production")]
public sealed class ProductionController(IProductionService productionService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken cancellationToken)
    {
        var overview = await productionService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpGet("sectors/{sector}")]
    public async Task<IActionResult> Sector(string sector, CancellationToken cancellationToken)
    {
        var detail = await productionService.GetSectorAsync(sector, cancellationToken);
        return Ok(detail);
    }

    [HttpPost("orders/{productionOrderId:guid}/advance")]
    public async Task<IActionResult> Advance(Guid productionOrderId, CancellationToken cancellationToken)
    {
        var overview = await productionService.AdvanceAsync(productionOrderId, cancellationToken);
        return Ok(overview);
    }

    [HttpPost("orders/{productionOrderId:guid}/split")]
    public async Task<IActionResult> Split(
        Guid productionOrderId,
        [FromBody] SplitProductionOrderRequest request,
        CancellationToken cancellationToken)
    {
        var overview = await productionService.SplitAsync(productionOrderId, request, cancellationToken);
        return Ok(overview);
    }

    [HttpPost("orders/merge")]
    public async Task<IActionResult> Merge([FromBody] MergeProductionOrdersRequest request, CancellationToken cancellationToken)
    {
        var overview = await productionService.MergeAsync(request, cancellationToken);
        return Ok(overview);
    }
}
