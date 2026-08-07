using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.InventoryHistory;

namespace GroceryManager.Api.Services.InventoryHistory;

public interface IInventoryAdjustmentService
{
    public Task<PagedResponse<InventoryAdjustmentResponse>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    public Task<InventoryAdjustmentResponse> GetAsync(Guid adjustmentId, CancellationToken cancellationToken);
    public Task<InventoryAdjustmentResponse> CreateAsync(
        CreateInventoryAdjustmentRequest request,
        CancellationToken cancellationToken);
    public Task<InventoryAdjustmentResponse> ReverseAsync(
        Guid adjustmentId,
        ReverseInventoryAdjustmentRequest request,
        CancellationToken cancellationToken);
}
