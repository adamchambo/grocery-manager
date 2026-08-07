namespace GroceryManager.Api.Entities.Pantry;

public sealed class PantryItemLocation
{
    public Guid Id { get; set; }
    public Guid PantryItemId { get; set; }
    public Guid StorageLocationId { get; set; }
    public decimal CurrentQuantity { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset? LastConfirmedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
