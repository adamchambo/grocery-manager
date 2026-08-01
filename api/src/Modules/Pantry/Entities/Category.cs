namespace GroceryManager.Modules.Pantry.Entities;

public sealed class Category
{
    public Guid Id { get; set; }
    public Guid PantryId { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
