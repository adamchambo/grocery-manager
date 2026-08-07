using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public interface IPantryItemService
{
    public Task<PagedResponse<PantryItemResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken);

    public Task<PantryItemResponse> GetAsync(Guid itemId, CancellationToken cancellationToken);
    public Task<PantryItemResponse> CreateAsync(CreatePantryItemRequest request, CancellationToken cancellationToken);
    public Task<PantryItemResponse> UpdateAsync(
        Guid itemId,
        UpdatePantryItemRequest request,
        CancellationToken cancellationToken);
    public Task ArchiveAsync(Guid itemId, CancellationToken cancellationToken);
    public Task<PantryItemResponse> UpdateLocationsAsync(
        Guid itemId,
        UpdatePantryItemLocationsRequest request,
        CancellationToken cancellationToken);
}
