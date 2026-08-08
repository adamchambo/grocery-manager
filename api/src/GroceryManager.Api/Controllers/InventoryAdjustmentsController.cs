using System.ComponentModel.DataAnnotations;
using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.InventoryHistory;
using GroceryManager.Api.Services.InventoryHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory-adjustments")]
public sealed class InventoryAdjustmentsController(IInventoryAdjustmentService inventoryAdjustmentService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<InventoryAdjustmentResponse>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 25,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryAdjustmentService.ListAsync(page, pageSize, cancellationToken));

    [HttpGet("{adjustmentId:guid}")]
    public async Task<ActionResult<InventoryAdjustmentResponse>> Get(
        Guid adjustmentId,
        CancellationToken cancellationToken) =>
        Ok(await inventoryAdjustmentService.GetAsync(adjustmentId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<InventoryAdjustmentResponse>> Create(
        CreateInventoryAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        var adjustment = await inventoryAdjustmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { adjustmentId = adjustment.Id }, adjustment);
    }

    [HttpPost("{adjustmentId:guid}/reverse")]
    public async Task<ActionResult<InventoryAdjustmentResponse>> Reverse(
        Guid adjustmentId,
        ReverseInventoryAdjustmentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await inventoryAdjustmentService.ReverseAsync(adjustmentId, request, cancellationToken));
}
