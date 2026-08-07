namespace GroceryManager.Api.Services.Documents;

public interface IShoppingListDocumentService
{
    public Task<byte[]> GeneratePdfAsync(Guid listId, CancellationToken cancellationToken);
}
