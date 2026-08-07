using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Stocktakes;

namespace GroceryManager.Api.Services.Stocktakes;

public interface IStocktakeService
{
    public Task<PagedResponse<StocktakeResponse>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    public Task<StocktakeResponse> GetAsync(Guid stocktakeId, CancellationToken cancellationToken);
    public Task<StocktakeResponse> StartAsync(StartStocktakeRequest request, CancellationToken cancellationToken);
    public Task<StocktakeEntryResponse> UpdateEntryAsync(
        Guid stocktakeId,
        Guid entryId,
        UpdateStocktakeEntryRequest request,
        CancellationToken cancellationToken);
    public Task<StocktakeEntryResponse> AddDiscoveredItemAsync(
        Guid stocktakeId,
        AddDiscoveredStocktakeItemRequest request,
        CancellationToken cancellationToken);
    public Task<StocktakeResponse> CompleteAsync(Guid stocktakeId, CancellationToken cancellationToken);
    public Task<StocktakeResponse> CancelAsync(Guid stocktakeId, CancellationToken cancellationToken);
}
