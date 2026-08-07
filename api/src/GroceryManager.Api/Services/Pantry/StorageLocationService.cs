using GroceryManager.Api.Dtos.Pantry;

namespace GroceryManager.Api.Services.Pantry;

public sealed class StorageLocationService : IStorageLocationService
{
    public Task<IReadOnlyList<StorageLocationResponse>> ListAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<StorageLocationResponse> CreateAsync(CreateStorageLocationRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<StorageLocationResponse> UpdateAsync(Guid locationId, UpdateStorageLocationRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task ArchiveAsync(Guid locationId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task UpdateOrderAsync(UpdateStorageLocationOrderRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task UpdateItemOrderAsync(Guid locationId, UpdateLocationItemOrderRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
