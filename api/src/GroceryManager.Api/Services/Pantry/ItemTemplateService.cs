using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public sealed class ItemTemplateService : IItemTemplateService
{
    public Task<IReadOnlyList<ItemTemplateResponse>> ListActiveAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}
