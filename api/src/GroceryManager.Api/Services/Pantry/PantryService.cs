using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public sealed class PantryService : IPantryService
{
    public Task<PantryResponse> CreateAsync(CreatePantryRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PantryResponse> GetCurrentAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PantryResponse> UpdateCurrentAsync(UpdatePantryRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
