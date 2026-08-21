using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Inventory;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/stock")]
public sealed class StockController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var items = await inventoryService.ListStockAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("{stockItemId:guid}/reserve")]
    public async Task<IActionResult> Reserve(Guid stockItemId, [FromBody] QuantityRequest request, CancellationToken cancellationToken)
    {
        var items = await inventoryService.ReserveAsync(stockItemId, request.Quantity, cancellationToken);
        return Ok(items);
    }

    [HttpPost("{stockItemId:guid}/replenish")]
    public async Task<IActionResult> Replenish(Guid stockItemId, [FromBody] QuantityRequest request, CancellationToken cancellationToken)
    {
        var items = await inventoryService.ReplenishAsync(stockItemId, request.Quantity, cancellationToken);
        return Ok(items);
    }

    public sealed record QuantityRequest(decimal Quantity);
}
