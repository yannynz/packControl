using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Orders;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var orders = await orderService.ListAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { orderId = order.Id }, order);
    }

    [HttpPost("{orderId:guid}/attachments")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> AttachFile(
        Guid orderId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { message = "Arquivo vazio." });
        }

        await using var stream = file.OpenReadStream();
        var order = await orderService.AttachFileAsync(orderId, stream, file.FileName, file.ContentType, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{orderId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orderService.ApproveAsync(orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}
