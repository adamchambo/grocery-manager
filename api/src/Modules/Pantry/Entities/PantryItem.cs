using GroceryManager.Modules.Pantry.Enums;

namespace GroceryManager.Modules.Pantry.Entities;

public sealed class PantryItem
{
    public Guid Id { get; set; }
    public Guid PantryId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? SourceTemplateId { get; set; }
    public Guid? DefaultStorageLocationId { get; set; }
    public required string Name { get; set; }
    public string? Brand { get; set; }
    public string? PreferredProduct { get; set; }
    public string? Notes { get; set; }
    public TrackingUnit TrackingUnit { get; set; }
    public decimal? PackageSize { get; set; }
    public string? PackageUnit { get; set; }
    public decimal? ConsumptionQuantity { get; set; }
    public decimal? ConsumptionPeriodDays { get; set; }
    public decimal BufferDays { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] Version { get; set; } = [];
}
