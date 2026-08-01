namespace GroceryManager.Modules.Stocktakes.Entities;

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
