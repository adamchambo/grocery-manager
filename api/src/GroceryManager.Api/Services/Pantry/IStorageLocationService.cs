using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public interface IStorageLocationService
{
    public Task<IReadOnlyList<StorageLocationResponse>> ListAsync(CancellationToken cancellationToken);
    public Task<StorageLocationResponse> CreateAsync(
        CreateStorageLocationRequest request,
        CancellationToken cancellationToken);
    public Task<StorageLocationResponse> UpdateAsync(
        Guid locationId,
        UpdateStorageLocationRequest request,
        CancellationToken cancellationToken);
    public Task ArchiveAsync(Guid locationId, CancellationToken cancellationToken);
    public Task UpdateOrderAsync(UpdateStorageLocationOrderRequest request, CancellationToken cancellationToken);
    public Task UpdateItemOrderAsync(
        Guid locationId,
        UpdateLocationItemOrderRequest request,
        CancellationToken cancellationToken);
}
