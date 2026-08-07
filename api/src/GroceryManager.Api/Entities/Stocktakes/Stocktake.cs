using GroceryManager.Api.Enums.Stocktakes;

namespace GroceryManager.Api.Entities.Stocktakes;

public sealed class Stocktake
{
    public Guid Id { get; set; }
    public Guid PantryId { get; set; }
    public Guid? ShoppingPresetId { get; set; }
    public StocktakeStatus Status { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
