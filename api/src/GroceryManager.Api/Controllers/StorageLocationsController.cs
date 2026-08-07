using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/storage-locations")]
public sealed class StorageLocationsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Create() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{locationId:guid}")]
    public IActionResult Update(Guid locationId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpDelete("{locationId:guid}")]
    public IActionResult Archive(Guid locationId) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("order")]
    public IActionResult UpdateOrder() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("{locationId:guid}/item-order")]
    public IActionResult UpdateItemOrder(Guid locationId) => StatusCode(StatusCodes.Status501NotImplemented);
}
