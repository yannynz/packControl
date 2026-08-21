using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Assets;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/assets")]
public sealed class AssetsController(IAssetService assetService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var assets = await assetService.ListAsync(cancellationToken);
        return Ok(assets);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTechnicalAssetRequest request, CancellationToken cancellationToken)
    {
        var asset = await assetService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { asset.Id }, asset);
    }

    [HttpPut("{assetId:guid}")]
    public async Task<IActionResult> Update(Guid assetId, [FromBody] UpdateTechnicalAssetRequest request, CancellationToken cancellationToken)
    {
        var asset = await assetService.UpdateAsync(assetId, request, cancellationToken);
        return asset is null ? NotFound() : Ok(asset);
    }
}
