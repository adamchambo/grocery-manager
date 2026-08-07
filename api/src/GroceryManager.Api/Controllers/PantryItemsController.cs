using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/pantry-items")]
public sealed class PantryItemsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Create() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{itemId:guid}")]
    public IActionResult Get(Guid itemId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{itemId:guid}")]
    public IActionResult Update(Guid itemId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpDelete("{itemId:guid}")]
    public IActionResult Archive(Guid itemId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{itemId:guid}/locations")]
    public IActionResult UpdateLocations(Guid itemId) => StatusCode(StatusCodes.Status501NotImplemented);
}
