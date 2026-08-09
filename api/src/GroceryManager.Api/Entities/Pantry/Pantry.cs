namespace GroceryManager.Api.Entities.Pantry;

public sealed class Pantry
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public required string Name { get; set; }
    public string? PrimaryShopName { get; set; }
    public decimal ShoppingIntervalDays { get; set; } = 14;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
