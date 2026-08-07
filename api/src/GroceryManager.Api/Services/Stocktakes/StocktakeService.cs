using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Stocktakes;

namespace GroceryManager.Api.Services.Stocktakes;

public sealed class StocktakeService : IStocktakeService
{
    public Task<PagedResponse<StocktakeResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<StocktakeResponse> GetAsync(Guid stocktakeId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<StocktakeResponse> StartAsync(StartStocktakeRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<StocktakeEntryResponse> UpdateEntryAsync(Guid stocktakeId, Guid entryId, UpdateStocktakeEntryRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<StocktakeEntryResponse> AddDiscoveredItemAsync(Guid stocktakeId, AddDiscoveredStocktakeItemRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<StocktakeResponse> CompleteAsync(Guid stocktakeId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<StocktakeResponse> CancelAsync(Guid stocktakeId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
