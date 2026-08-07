using GroceryManager.Api.Enums.Pantry;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record PantryItemLocationRequest(
    Guid StorageLocationId,
    decimal CurrentQuantity,
    int SortOrder);

public sealed record CreatePantryItemRequest(
    Guid CategoryId,
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
    IReadOnlyList<PantryItemLocationRequest> Locations);

public sealed record UpdatePantryItemRequest(
    Guid CategoryId,
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
    string Version);

public sealed record UpdatePantryItemLocationsRequest(
    IReadOnlyList<PantryItemLocationRequest> Locations,
    string ItemVersion);

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
