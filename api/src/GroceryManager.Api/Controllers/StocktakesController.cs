using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/stocktakes")]
public sealed class StocktakesController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Start() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{stocktakeId:guid}")]
    public IActionResult Get(Guid stocktakeId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{stocktakeId:guid}/entries/{entryId:guid}")]
    public IActionResult UpdateEntry(Guid stocktakeId, Guid entryId) =>
        StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{stocktakeId:guid}/discovered-items")]
    public IActionResult AddDiscoveredItem(Guid stocktakeId) =>
        StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{stocktakeId:guid}/complete")]
    public IActionResult Complete(Guid stocktakeId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{stocktakeId:guid}/cancel")]
    public IActionResult Cancel(Guid stocktakeId) => StatusCode(StatusCodes.Status501NotImplemented);
}
