using GroceryManager.Api.Enums.Shopping;

namespace GroceryManager.Api.Entities.Shopping;

public sealed class ShoppingListItem
{
    public Guid Id { get; set; }
    public Guid ShoppingListId { get; set; }
    public Guid? PantryItemId { get; set; }
    public Guid? DestinationLocationId { get; set; }
    public required string ItemNameSnapshot { get; set; }
    public string? BrandSnapshot { get; set; }
    public string? CategoryNameSnapshot { get; set; }
    public string? TrackingUnitSnapshot { get; set; }
    public decimal? PackageSizeSnapshot { get; set; }
    public string? PackageUnitSnapshot { get; set; }
    public decimal? StockAtGeneration { get; set; }
    public decimal? RequiredAtGeneration { get; set; }
    public decimal? SuggestedPurchaseQuantity { get; set; }
    public decimal? ActualPurchaseQuantity { get; set; }
    public ShoppingListItemOutcome Outcome { get; set; }
    public bool IsManual { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset? InventoryAppliedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
