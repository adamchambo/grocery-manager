using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public interface IPantryService
{
    public Task<PantryResponse> CreateAsync(CreatePantryRequest request, CancellationToken cancellationToken);
    public Task<PantryResponse> GetCurrentAsync(CancellationToken cancellationToken);
    public Task<PantryResponse> UpdateCurrentAsync(UpdatePantryRequest request, CancellationToken cancellationToken);
}
