namespace GroceryManager.Api.Entities.ShoppingPresets;

public sealed class ShoppingPreset
{
    public Guid Id { get; set; }
    public Guid PantryId { get; set; }
    public required string Name { get; set; }
    public decimal CoverageDays { get; set; }
    public bool IsEverythingPreset { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
