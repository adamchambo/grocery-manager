using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public sealed class CategoryService : ICategoryService
{
    public Task<IReadOnlyList<CategoryResponse>> ListAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<CategoryResponse> UpdateAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task ArchiveAsync(Guid categoryId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task UpdateOrderAsync(UpdateCategoryOrderRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
