using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/item-templates")]
public sealed class ItemTemplatesController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(StatusCodes.Status501NotImplemented);
}
