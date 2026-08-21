using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Products;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var products = await productService.ListAsync(cancellationToken);
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductTemplateRequest request, CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { product.Id }, product);
    }

    [HttpPut("{productTemplateId:guid}")]
    public async Task<IActionResult> Update(
        Guid productTemplateId,
        [FromBody] UpdateProductTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateAsync(productTemplateId, request, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}
