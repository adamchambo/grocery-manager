using System.ComponentModel.DataAnnotations;
using GroceryManager.Api.Enums.Pantry;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record PantryItemLocationRequest(
    Guid StorageLocationId,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal CurrentQuantity,
    [Range(0, int.MaxValue)] int SortOrder);

public sealed record CreatePantryItemRequest(
    Guid CategoryId,
    Guid? SourceTemplateId,
    Guid? DefaultStorageLocationId,
    [Required, StringLength(160)] string Name,
    [StringLength(100)] string? Icon,
    [StringLength(120)] string? Brand,
    [StringLength(200)] string? PreferredProduct,
    [StringLength(2000)] string? Notes,
    TrackingUnit TrackingUnit,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal? PackageSize,
    [StringLength(32)] string? PackageUnit,
    [Range(typeof(decimal), "0.001", "999999999999999.999")] decimal? ConsumptionQuantity,
    [Range(typeof(decimal), "0.001", "999999999999999.999")] decimal? ConsumptionPeriodDays,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal BufferDays,
    [Required, MinLength(1)] IReadOnlyList<PantryItemLocationRequest> Locations) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ConsumptionQuantity.HasValue != ConsumptionPeriodDays.HasValue)
            yield return new ValidationResult(
                "Consumption quantity and period days must be supplied together.",
                [nameof(ConsumptionQuantity), nameof(ConsumptionPeriodDays)]);
    }
}

public sealed record UpdatePantryItemRequest(
    Guid CategoryId,
    Guid? DefaultStorageLocationId,
    [Required, StringLength(160)] string Name,
    [StringLength(100)] string? Icon,
    [StringLength(120)] string? Brand,
    [StringLength(200)] string? PreferredProduct,
    [StringLength(2000)] string? Notes,
    TrackingUnit TrackingUnit,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal? PackageSize,
    [StringLength(32)] string? PackageUnit,
    [Range(typeof(decimal), "0.001", "999999999999999.999")] decimal? ConsumptionQuantity,
    [Range(typeof(decimal), "0.001", "999999999999999.999")] decimal? ConsumptionPeriodDays,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal BufferDays,
    [Required] string Version) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ConsumptionQuantity.HasValue != ConsumptionPeriodDays.HasValue)
            yield return new ValidationResult(
                "Consumption quantity and period days must be supplied together.",
                [nameof(ConsumptionQuantity), nameof(ConsumptionPeriodDays)]);
    }
}

public sealed record UpdatePantryItemLocationsRequest(
    [Required, MinLength(1)] IReadOnlyList<PantryItemLocationRequest> Locations,
    [Required] string ItemVersion);

public sealed record PantryItemLocationResponse(
    Guid Id,
    Guid StorageLocationId,
    string StorageLocationName,
    decimal CurrentQuantity,
    int SortOrder,
    DateTimeOffset? LastConfirmedAtUtc,
    string Version);

public sealed record PantryItemResponse(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid? SourceTemplateId,
    Guid? DefaultStorageLocationId,
    string Name,
    string? Icon,
    string? Brand,
    string? PreferredProduct,
    string? Notes,
    TrackingUnit TrackingUnit,
    decimal? PackageSize,
    string? PackageUnit,
    decimal? ConsumptionQuantity,
    decimal? ConsumptionPeriodDays,
    decimal BufferDays,
    bool IsArchived,
    IReadOnlyList<PantryItemLocationResponse> Locations,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string Version);
