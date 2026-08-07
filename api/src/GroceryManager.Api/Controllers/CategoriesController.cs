using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Create() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{categoryId:guid}")]
    public IActionResult Update(Guid categoryId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpDelete("{categoryId:guid}")]
    public IActionResult Archive(Guid categoryId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("order")]
    public IActionResult UpdateOrder() => StatusCode(StatusCodes.Status501NotImplemented);
}
