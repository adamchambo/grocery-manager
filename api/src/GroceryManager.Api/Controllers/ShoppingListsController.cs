using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/shopping-lists")]
public sealed class ShoppingListsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Generate() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{listId:guid}")]
    public IActionResult Get(Guid listId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{listId:guid}")]
    public IActionResult Update(Guid listId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{listId:guid}/items")]
    public IActionResult AddItem(Guid listId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{listId:guid}/items/{itemId:guid}")]
    public IActionResult UpdateItem(Guid listId, Guid itemId) =>
        StatusCode(StatusCodes.Status501NotImplemented);

    [HttpDelete("{listId:guid}/items/{itemId:guid}")]
    public IActionResult RemoveItem(Guid listId, Guid itemId) =>
        StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{listId:guid}/recalculate")]
    public IActionResult Recalculate(Guid listId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{listId:guid}/complete")]
    public IActionResult Complete(Guid listId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{listId:guid}/undo")]
    public IActionResult Undo(Guid listId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{listId:guid}/pdf")]
    public IActionResult DownloadPdf(Guid listId) => StatusCode(StatusCodes.Status501NotImplemented);
}
