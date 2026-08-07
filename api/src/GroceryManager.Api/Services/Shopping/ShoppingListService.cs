using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Shopping;
using GroceryManager.Api.Enums.Shopping;

namespace GroceryManager.Api.Services.Shopping;

public sealed class ShoppingListService : IShoppingListService
{
    public Task<PagedResponse<ShoppingListResponse>> ListAsync(int page, int pageSize, ShoppingListStatus? status, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingListResponse> GetAsync(Guid listId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingListResponse> GenerateAsync(GenerateShoppingListRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingListResponse> UpdateAsync(Guid listId, UpdateShoppingListRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingListItemResponse> AddItemAsync(Guid listId, AddShoppingListItemRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingListItemResponse> UpdateItemAsync(Guid listId, Guid itemId, UpdateShoppingListItemRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task RemoveItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingListResponse> RecalculateAsync(Guid listId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingListResponse> CompleteAsync(Guid listId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingListResponse> UndoAsync(Guid listId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
