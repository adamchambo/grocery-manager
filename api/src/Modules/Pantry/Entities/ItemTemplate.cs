namespace GroceryManager.Modules.Pantry.Entities;

public sealed class ItemTemplate
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string DefaultCategoryKey { get; set; }
    public TrackingUnit DefaultTrackingUnit { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
