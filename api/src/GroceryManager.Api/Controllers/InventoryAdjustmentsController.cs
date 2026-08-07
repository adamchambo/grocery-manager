using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/inventory-adjustments")]
public sealed class InventoryAdjustmentsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{adjustmentId:guid}")]
    public IActionResult Get(Guid adjustmentId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Create() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{adjustmentId:guid}/reverse")]
    public IActionResult Reverse(Guid adjustmentId) => StatusCode(StatusCodes.Status501NotImplemented);
}
