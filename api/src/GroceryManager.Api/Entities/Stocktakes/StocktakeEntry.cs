using GroceryManager.Api.Enums.Stocktakes;

namespace GroceryManager.Api.Entities.Stocktakes;

public sealed class StocktakeEntry
{
    public Guid Id { get; set; }
    public Guid StocktakeId { get; set; }
    public Guid PantryItemLocationId { get; set; }
    public required string ItemNameSnapshot { get; set; }
    public required string LocationNameSnapshot { get; set; }
    public required string TrackingUnitSnapshot { get; set; }
    public int LocationSortOrderSnapshot { get; set; }
    public int ItemSortOrderSnapshot { get; set; }
    public decimal PreviousConfirmedQuantity { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal? RecordedQuantity { get; set; }
    public StocktakeEntryStatus Status { get; set; }
    public bool IsOutlier { get; set; }
    public DateTimeOffset? ConfirmedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
