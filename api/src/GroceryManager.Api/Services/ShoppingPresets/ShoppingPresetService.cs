using GroceryManager.Api.Dtos.ShoppingPresets;

namespace GroceryManager.Api.Services.ShoppingPresets;

public sealed class ShoppingPresetService : IShoppingPresetService
{
    public Task<IReadOnlyList<ShoppingPresetResponse>> ListAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingPresetResponse> GetAsync(Guid presetId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingPresetResponse> CreateAsync(CreateShoppingPresetRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingPresetResponse> UpdateAsync(Guid presetId, UpdateShoppingPresetRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task ArchiveAsync(Guid presetId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ShoppingPresetPreviewResponse> PreviewAsync(Guid presetId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
