namespace GroceryManager.Api.Services.Documents;

public sealed class ShoppingListDocumentService : IShoppingListDocumentService
{
    public Task<byte[]> GeneratePdfAsync(Guid listId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
