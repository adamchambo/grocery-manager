using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Services.Pantry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/item-templates")]
public sealed class ItemTemplatesController(IItemTemplateService itemTemplateService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ItemTemplateResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await itemTemplateService.ListActiveAsync(cancellationToken));
}
