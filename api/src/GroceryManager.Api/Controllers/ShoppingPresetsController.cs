using GroceryManager.Api.Dtos.ShoppingPresets;
using GroceryManager.Api.Services.ShoppingPresets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shopping-presets")]
public sealed class ShoppingPresetsController(IShoppingPresetService shoppingPresetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShoppingPresetResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await shoppingPresetService.ListAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ShoppingPresetResponse>> Create(
        CreateShoppingPresetRequest request,
        CancellationToken cancellationToken)
    {
        var preset = await shoppingPresetService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { presetId = preset.Id }, preset);
    }

    [HttpGet("{presetId:guid}")]
    public async Task<ActionResult<ShoppingPresetResponse>> Get(
        Guid presetId,
        CancellationToken cancellationToken) =>
        Ok(await shoppingPresetService.GetAsync(presetId, cancellationToken));

    [HttpPut("{presetId:guid}")]
    public async Task<ActionResult<ShoppingPresetResponse>> Update(
        Guid presetId,
        UpdateShoppingPresetRequest request,
        CancellationToken cancellationToken) =>
        Ok(await shoppingPresetService.UpdateAsync(presetId, request, cancellationToken));

    [HttpDelete("{presetId:guid}")]
    public async Task<IActionResult> Archive(Guid presetId, CancellationToken cancellationToken)
    {
        await shoppingPresetService.ArchiveAsync(presetId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{presetId:guid}/preview")]
    public async Task<ActionResult<ShoppingPresetPreviewResponse>> Preview(
        Guid presetId,
        CancellationToken cancellationToken) =>
        Ok(await shoppingPresetService.PreviewAsync(presetId, cancellationToken));
}
