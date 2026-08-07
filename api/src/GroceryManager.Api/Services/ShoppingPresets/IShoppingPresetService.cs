using GroceryManager.Api.Dtos.ShoppingPresets;

namespace GroceryManager.Api.Services.ShoppingPresets;

public interface IShoppingPresetService
{
    public Task<IReadOnlyList<ShoppingPresetResponse>> ListAsync(CancellationToken cancellationToken);
    public Task<ShoppingPresetResponse> GetAsync(Guid presetId, CancellationToken cancellationToken);
    public Task<ShoppingPresetResponse> CreateAsync(
        CreateShoppingPresetRequest request,
        CancellationToken cancellationToken);
    public Task<ShoppingPresetResponse> UpdateAsync(
        Guid presetId,
        UpdateShoppingPresetRequest request,
        CancellationToken cancellationToken);
    public Task ArchiveAsync(Guid presetId, CancellationToken cancellationToken);
    public Task<ShoppingPresetPreviewResponse> PreviewAsync(Guid presetId, CancellationToken cancellationToken);
}
