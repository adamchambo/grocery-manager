using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public interface IItemTemplateService
{
    public Task<IReadOnlyList<ItemTemplateResponse>> ListActiveAsync(CancellationToken cancellationToken);
}
