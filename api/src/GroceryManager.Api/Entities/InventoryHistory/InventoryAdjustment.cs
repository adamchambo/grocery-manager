using GroceryManager.Api.Enums.InventoryHistory;

namespace GroceryManager.Api.Entities.InventoryHistory;

public sealed class InventoryAdjustment
{
    public Guid Id { get; set; }
    public Guid PantryItemLocationId { get; set; }
    public Guid? SourceStocktakeEntryId { get; set; }
    public Guid? SourceShoppingListItemId { get; set; }
    public Guid? ReversesAdjustmentId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public InventoryAdjustmentType AdjustmentType { get; set; }
    public decimal QuantityDelta { get; set; }
    public string? Notes { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
