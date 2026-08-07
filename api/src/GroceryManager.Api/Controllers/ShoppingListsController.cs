using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Shopping;
using GroceryManager.Api.Enums.Shopping;
using GroceryManager.Api.Services.Documents;
using GroceryManager.Api.Services.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shopping-lists")]
public sealed class ShoppingListsController(
    IShoppingListService shoppingListService,
    IShoppingListDocumentService documentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ShoppingListResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] ShoppingListStatus? status = null,
        CancellationToken cancellationToken = default) =>
        Ok(await shoppingListService.ListAsync(page, pageSize, status, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ShoppingListResponse>> Generate(
        GenerateShoppingListRequest request,
        CancellationToken cancellationToken)
    {
        var list = await shoppingListService.GenerateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { listId = list.Id }, list);
    }

    [HttpGet("{listId:guid}")]
    public async Task<ActionResult<ShoppingListResponse>> Get(Guid listId, CancellationToken cancellationToken) =>
        Ok(await shoppingListService.GetAsync(listId, cancellationToken));

    [HttpPut("{listId:guid}")]
    public async Task<ActionResult<ShoppingListResponse>> Update(
        Guid listId,
        UpdateShoppingListRequest request,
        CancellationToken cancellationToken) =>
        Ok(await shoppingListService.UpdateAsync(listId, request, cancellationToken));

    [HttpPost("{listId:guid}/items")]
    public async Task<ActionResult<ShoppingListItemResponse>> AddItem(
        Guid listId,
        AddShoppingListItemRequest request,
        CancellationToken cancellationToken) =>
        Ok(await shoppingListService.AddItemAsync(listId, request, cancellationToken));

    [HttpPut("{listId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ShoppingListItemResponse>> UpdateItem(
        Guid listId,
        Guid itemId,
        UpdateShoppingListItemRequest request,
        CancellationToken cancellationToken) =>
        Ok(await shoppingListService.UpdateItemAsync(listId, itemId, request, cancellationToken));

    [HttpDelete("{listId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        await shoppingListService.RemoveItemAsync(listId, itemId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{listId:guid}/recalculate")]
    public async Task<ActionResult<ShoppingListResponse>> Recalculate(
        Guid listId,
        CancellationToken cancellationToken) =>
        Ok(await shoppingListService.RecalculateAsync(listId, cancellationToken));

    [HttpPost("{listId:guid}/complete")]
    public async Task<ActionResult<ShoppingListResponse>> Complete(
        Guid listId,
        CancellationToken cancellationToken) =>
        Ok(await shoppingListService.CompleteAsync(listId, cancellationToken));

    [HttpPost("{listId:guid}/undo")]
    public async Task<ActionResult<ShoppingListResponse>> Undo(
        Guid listId,
        CancellationToken cancellationToken) =>
        Ok(await shoppingListService.UndoAsync(listId, cancellationToken));

    [HttpGet("{listId:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid listId, CancellationToken cancellationToken)
    {
        var content = await documentService.GeneratePdfAsync(listId, cancellationToken);
        return File(content, "application/pdf", $"shopping-list-{listId}.pdf");
    }
}
