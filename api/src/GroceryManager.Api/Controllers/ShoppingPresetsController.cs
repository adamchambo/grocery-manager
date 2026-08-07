using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/shopping-presets")]
public sealed class ShoppingPresetsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Create() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{presetId:guid}")]
    public IActionResult Get(Guid presetId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{presetId:guid}")]
    public IActionResult Update(Guid presetId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpDelete("{presetId:guid}")]
    public IActionResult Archive(Guid presetId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{presetId:guid}/preview")]
    public IActionResult Preview(Guid presetId) => StatusCode(StatusCodes.Status501NotImplemented);
}
