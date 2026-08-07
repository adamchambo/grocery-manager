using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public interface ICategoryService
{
    public Task<IReadOnlyList<CategoryResponse>> ListAsync(CancellationToken cancellationToken);
    public Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    public Task<CategoryResponse> UpdateAsync(
        Guid categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken);
    public Task ArchiveAsync(Guid categoryId, CancellationToken cancellationToken);
    public Task UpdateOrderAsync(UpdateCategoryOrderRequest request, CancellationToken cancellationToken);
}
