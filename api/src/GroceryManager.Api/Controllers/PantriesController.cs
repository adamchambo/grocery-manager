using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/pantries")]
public sealed class PantriesController : ControllerBase
{
    [HttpPost]
    public IActionResult Create() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("current")]
    public IActionResult GetCurrent() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("current")]
    public IActionResult UpdateCurrent() => StatusCode(StatusCodes.Status501NotImplemented);
}
