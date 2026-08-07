using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Services.Pantry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pantries")]
public sealed class PantriesController(IPantryService pantryService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PantryResponse>> Create(
        CreatePantryRequest request,
        CancellationToken cancellationToken)
    {
        var pantry = await pantryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCurrent), pantry);
    }

    [HttpGet("current")]
    public async Task<ActionResult<PantryResponse>> GetCurrent(CancellationToken cancellationToken) =>
        Ok(await pantryService.GetCurrentAsync(cancellationToken));

    [HttpPut("current")]
    public async Task<ActionResult<PantryResponse>> UpdateCurrent(
        UpdatePantryRequest request,
        CancellationToken cancellationToken) =>
        Ok(await pantryService.UpdateCurrentAsync(request, cancellationToken));
}
