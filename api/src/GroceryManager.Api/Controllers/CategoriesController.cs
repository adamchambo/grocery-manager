using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Services.Pantry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await categoryService.ListAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(
        CreateCategoryRequest request,
        CancellationToken cancellationToken) =>
        Ok(await categoryService.CreateAsync(request, cancellationToken));

    [HttpPut("{categoryId:guid}")]
    public async Task<ActionResult<CategoryResponse>> Update(
        Guid categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken) =>
        Ok(await categoryService.UpdateAsync(categoryId, request, cancellationToken));

    [HttpDelete("{categoryId:guid}")]
    public async Task<IActionResult> Archive(Guid categoryId, CancellationToken cancellationToken)
    {
        await categoryService.ArchiveAsync(categoryId, cancellationToken);
        return NoContent();
    }

    [HttpPut("order")]
    public async Task<IActionResult> UpdateOrder(
        UpdateCategoryOrderRequest request,
        CancellationToken cancellationToken)
    {
        await categoryService.UpdateOrderAsync(request, cancellationToken);
        return NoContent();
    }
}
