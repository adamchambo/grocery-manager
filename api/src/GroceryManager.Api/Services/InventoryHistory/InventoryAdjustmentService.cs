using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.InventoryHistory;

namespace GroceryManager.Api.Services.InventoryHistory;

public sealed class InventoryAdjustmentService : IInventoryAdjustmentService
{
    public Task<PagedResponse<InventoryAdjustmentResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<InventoryAdjustmentResponse> GetAsync(Guid adjustmentId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<InventoryAdjustmentResponse> CreateAsync(CreateInventoryAdjustmentRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<InventoryAdjustmentResponse> ReverseAsync(Guid adjustmentId, ReverseInventoryAdjustmentRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
