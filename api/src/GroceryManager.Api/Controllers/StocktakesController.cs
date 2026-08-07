using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Stocktakes;
using GroceryManager.Api.Services.Stocktakes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/stocktakes")]
public sealed class StocktakesController(IStocktakeService stocktakeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<StocktakeResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default) =>
        Ok(await stocktakeService.ListAsync(page, pageSize, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<StocktakeResponse>> Start(
        StartStocktakeRequest request,
        CancellationToken cancellationToken)
    {
        var stocktake = await stocktakeService.StartAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { stocktakeId = stocktake.Id }, stocktake);
    }

    [HttpGet("{stocktakeId:guid}")]
    public async Task<ActionResult<StocktakeResponse>> Get(
        Guid stocktakeId,
        CancellationToken cancellationToken) =>
        Ok(await stocktakeService.GetAsync(stocktakeId, cancellationToken));

    [HttpPut("{stocktakeId:guid}/entries/{entryId:guid}")]
    public async Task<ActionResult<StocktakeEntryResponse>> UpdateEntry(
        Guid stocktakeId,
        Guid entryId,
        UpdateStocktakeEntryRequest request,
        CancellationToken cancellationToken) =>
        Ok(await stocktakeService.UpdateEntryAsync(stocktakeId, entryId, request, cancellationToken));

    [HttpPost("{stocktakeId:guid}/discovered-items")]
    public async Task<ActionResult<StocktakeEntryResponse>> AddDiscoveredItem(
        Guid stocktakeId,
        AddDiscoveredStocktakeItemRequest request,
        CancellationToken cancellationToken) =>
        Ok(await stocktakeService.AddDiscoveredItemAsync(stocktakeId, request, cancellationToken));

    [HttpPost("{stocktakeId:guid}/complete")]
    public async Task<ActionResult<StocktakeResponse>> Complete(
        Guid stocktakeId,
        CancellationToken cancellationToken) =>
        Ok(await stocktakeService.CompleteAsync(stocktakeId, cancellationToken));

    [HttpPost("{stocktakeId:guid}/cancel")]
    public async Task<ActionResult<StocktakeResponse>> Cancel(
        Guid stocktakeId,
        CancellationToken cancellationToken) =>
        Ok(await stocktakeService.CancelAsync(stocktakeId, cancellationToken));
}
