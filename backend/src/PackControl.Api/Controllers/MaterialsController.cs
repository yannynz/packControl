using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Inventory;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/materials")]
public sealed class MaterialsController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var materials = await inventoryService.ListMaterialsAsync(cancellationToken);
        return Ok(materials);
    }
}
