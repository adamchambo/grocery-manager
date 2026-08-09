using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Shopping;
using GroceryManager.Api.Enums.Shopping;

namespace GroceryManager.Api.Services.Shopping;

public interface IShoppingListService
{
    public Task<PagedResponse<ShoppingListResponse>> ListAsync(
        int page,
        int pageSize,
        ShoppingListStatus? status,
        CancellationToken cancellationToken);
    public Task<ShoppingListResponse> GetAsync(Guid listId, CancellationToken cancellationToken);
    public Task<ShoppingListResponse> GenerateAsync(
        GenerateShoppingListRequest request,
        CancellationToken cancellationToken);
    public Task<ShoppingListResponse> UpdateAsync(
        Guid listId,
        UpdateShoppingListRequest request,
        CancellationToken cancellationToken);
    public Task<ShoppingListItemResponse> AddItemAsync(
        Guid listId,
        AddShoppingListItemRequest request,
        CancellationToken cancellationToken);
    public Task<ShoppingListItemResponse> UpdateItemAsync(
        Guid listId,
        Guid itemId,
        UpdateShoppingListItemRequest request,
        CancellationToken cancellationToken);
    public Task UpdateOrderAsync(Guid listId, UpdateShoppingListOrderRequest request, CancellationToken cancellationToken);
    public Task RemoveItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken);
    public Task<ShoppingListItemResponse> UndoPurchaseAsync(Guid listId, Guid itemId, CancellationToken cancellationToken);
    public Task<ShoppingListResponse> RecalculateAsync(Guid listId, CancellationToken cancellationToken);
    public Task<ShoppingListResponse> CompleteAsync(Guid listId, CancellationToken cancellationToken);
    public Task<ShoppingListResponse> UndoAsync(Guid listId, CancellationToken cancellationToken);
}
