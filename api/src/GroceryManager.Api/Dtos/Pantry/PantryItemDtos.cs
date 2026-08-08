using System.ComponentModel.DataAnnotations;
using GroceryManager.Api.Enums.Pantry;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record PantryItemLocationRequest(
    Guid StorageLocationId,
    [property: Range(typeof(decimal), "0", "999999999999999.999")] decimal CurrentQuantity,
    [property: Range(0, int.MaxValue)] int SortOrder);

public sealed record CreatePantryItemRequest(
    Guid CategoryId,
    Guid? SourceTemplateId,
    Guid? DefaultStorageLocationId,
    [property: Required, StringLength(160)] string Name,
    [property: StringLength(120)] string? Brand,
    [property: StringLength(200)] string? PreferredProduct,
    [property: StringLength(2000)] string? Notes,
    TrackingUnit TrackingUnit,
    [property: Range(typeof(decimal), "0", "999999999999999.999")] decimal? PackageSize,
    [property: StringLength(32)] string? PackageUnit,
    [property: Range(typeof(decimal), "0.001", "999999999999999.999")] decimal? ConsumptionQuantity,
    [property: Range(typeof(decimal), "0.001", "999999999999999.999")] decimal? ConsumptionPeriodDays,
    [property: Range(typeof(decimal), "0", "999999999999999.999")] decimal BufferDays,
    [property: Required, MinLength(1)] IReadOnlyList<PantryItemLocationRequest> Locations) : IValidatableObject
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
    [property: Required, StringLength(160)] string Name,
    [property: StringLength(120)] string? Brand,
    [property: StringLength(200)] string? PreferredProduct,
    [property: StringLength(2000)] string? Notes,
    TrackingUnit TrackingUnit,
    [property: Range(typeof(decimal), "0", "999999999999999.999")] decimal? PackageSize,
    [property: StringLength(32)] string? PackageUnit,
    [property: Range(typeof(decimal), "0.001", "999999999999999.999")] decimal? ConsumptionQuantity,
    [property: Range(typeof(decimal), "0.001", "999999999999999.999")] decimal? ConsumptionPeriodDays,
    [property: Range(typeof(decimal), "0", "999999999999999.999")] decimal BufferDays,
    [property: Required] string Version) : IValidatableObject
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
    [property: Required, MinLength(1)] IReadOnlyList<PantryItemLocationRequest> Locations,
    [property: Required] string ItemVersion);

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
