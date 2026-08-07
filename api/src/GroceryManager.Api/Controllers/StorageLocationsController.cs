using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Services.Pantry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/storage-locations")]
public sealed class StorageLocationsController(IStorageLocationService storageLocationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StorageLocationResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await storageLocationService.ListAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<StorageLocationResponse>> Create(
        CreateStorageLocationRequest request,
        CancellationToken cancellationToken) =>
        Ok(await storageLocationService.CreateAsync(request, cancellationToken));

    [HttpPut("{locationId:guid}")]
    public async Task<ActionResult<StorageLocationResponse>> Update(
        Guid locationId,
        UpdateStorageLocationRequest request,
        CancellationToken cancellationToken) =>
        Ok(await storageLocationService.UpdateAsync(locationId, request, cancellationToken));

    [HttpDelete("{locationId:guid}")]
    public async Task<IActionResult> Archive(Guid locationId, CancellationToken cancellationToken)
    {
        await storageLocationService.ArchiveAsync(locationId, cancellationToken);
        return NoContent();
    }

    [HttpPut("order")]
    public async Task<IActionResult> UpdateOrder(
        UpdateStorageLocationOrderRequest request,
        CancellationToken cancellationToken)
    {
        await storageLocationService.UpdateOrderAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{locationId:guid}/item-order")]
    public async Task<IActionResult> UpdateItemOrder(
        Guid locationId,
        UpdateLocationItemOrderRequest request,
        CancellationToken cancellationToken)
    {
        await storageLocationService.UpdateItemOrderAsync(locationId, request, cancellationToken);
        return NoContent();
    }
}
