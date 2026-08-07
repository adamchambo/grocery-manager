using GroceryManager.Api.Enums.InventoryHistory;

namespace GroceryManager.Api.Dtos.InventoryHistory;

public sealed record CreateInventoryAdjustmentRequest(
    Guid PantryItemLocationId,
    decimal QuantityDelta,
    string? Notes,
    string IdempotencyKey);

public sealed record ReverseInventoryAdjustmentRequest(string? Notes, string IdempotencyKey);

public sealed record InventoryAdjustmentResponse(
    Guid Id,
    Guid PantryItemLocationId,
    Guid? SourceStocktakeEntryId,
    Guid? SourceShoppingListItemId,
    Guid? ReversesAdjustmentId,
    InventoryAdjustmentType AdjustmentType,
    decimal QuantityDelta,
    string? Notes,
    DateTimeOffset CreatedAtUtc);
