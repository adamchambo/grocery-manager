using System.ComponentModel.DataAnnotations;
using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Services.Pantry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pantry-items")]
public sealed class PantryItemsController(IPantryItemService pantryItemService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<PantryItemResponse>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 25,
        [FromQuery, StringLength(160)] string? search = null,
        CancellationToken cancellationToken = default) =>
        Ok(await pantryItemService.ListAsync(page, pageSize, search, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<PantryItemResponse>> Create(
        CreatePantryItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await pantryItemService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { itemId = item.Id }, item);
    }

    [HttpGet("{itemId:guid}")]
    public async Task<ActionResult<PantryItemResponse>> Get(Guid itemId, CancellationToken cancellationToken) =>
        Ok(await pantryItemService.GetAsync(itemId, cancellationToken));

    [HttpPut("{itemId:guid}")]
    public async Task<ActionResult<PantryItemResponse>> Update(
        Guid itemId,
        UpdatePantryItemRequest request,
        CancellationToken cancellationToken) =>
        Ok(await pantryItemService.UpdateAsync(itemId, request, cancellationToken));

    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> Archive(Guid itemId, CancellationToken cancellationToken)
    {
        await pantryItemService.ArchiveAsync(itemId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{itemId:guid}/locations")]
    public async Task<ActionResult<PantryItemResponse>> UpdateLocations(
        Guid itemId,
        UpdatePantryItemLocationsRequest request,
        CancellationToken cancellationToken) =>
        Ok(await pantryItemService.UpdateLocationsAsync(itemId, request, cancellationToken));
}
