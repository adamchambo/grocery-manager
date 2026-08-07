using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public sealed class PantryItemService : IPantryItemService
{
    public Task<PagedResponse<PantryItemResponse>> ListAsync(int page, int pageSize, string? search, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PantryItemResponse> GetAsync(Guid itemId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PantryItemResponse> CreateAsync(CreatePantryItemRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PantryItemResponse> UpdateAsync(Guid itemId, UpdatePantryItemRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task ArchiveAsync(Guid itemId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PantryItemResponse> UpdateLocationsAsync(Guid itemId, UpdatePantryItemLocationsRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
