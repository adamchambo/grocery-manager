using GroceryManager.Modules.Shopping.Enums;

namespace GroceryManager.Modules.Shopping.Entities;

public sealed class ShoppingList
{
    public Guid Id { get; set; }
    public Guid PantryId { get; set; }
    public Guid? SourcePresetId { get; set; }
    public Guid? SourceStocktakeId { get; set; }
    public required string Name { get; set; }
    public ShoppingListStatus Status { get; set; }
    public bool StockChangedSinceGeneration { get; set; }
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
