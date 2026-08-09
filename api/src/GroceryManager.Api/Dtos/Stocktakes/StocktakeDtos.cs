using System.ComponentModel.DataAnnotations;
using GroceryManager.Api.Enums.Pantry;
using GroceryManager.Api.Enums.Stocktakes;

namespace GroceryManager.Api.Dtos.Stocktakes;

public sealed record StartStocktakeRequest(Guid? ShoppingPresetId);

public sealed record UpdateStocktakeEntryRequest(
    StocktakeEntryStatus Status,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal? RecordedQuantity,
    [Required] string Version);

public sealed record SaveStocktakeLocationEntriesRequest(
    Guid StorageLocationId,
    [Required, MinLength(1)] IReadOnlyList<SaveStocktakeEntryRequest> Entries);

public sealed record SaveStocktakeEntryRequest(
    Guid EntryId,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal RecordedQuantity,
    [Required] string Version);

public sealed record AddDiscoveredStocktakeItemRequest(
    [Required, StringLength(160)] string Name,
    Guid CategoryId,
    Guid StorageLocationId,
    TrackingUnit TrackingUnit,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal RecordedQuantity);

public sealed record CompleteStocktakeRequest(
    IReadOnlyList<StocktakeLocationItemOrderRequest>? LocationItemOrders);

public sealed record StocktakeLocationItemOrderRequest(
    Guid StorageLocationId,
    [Required, MinLength(1)] IReadOnlyList<Guid> PantryItemLocationIds);

public sealed record UpdateStocktakeLocationOrderRequest(
    Guid StorageLocationId,
    [Required, MinLength(1)] IReadOnlyList<Guid> PantryItemLocationIds);

public sealed record StocktakeEntryResponse(
    Guid Id,
    Guid PantryItemLocationId,
    Guid StorageLocationId,
    string ItemName,
    string LocationName,
    string TrackingUnit,
    decimal PreviousConfirmedQuantity,
    decimal EstimatedQuantity,
    decimal? RecordedQuantity,
    StocktakeEntryStatus Status,
    bool IsOutlier,
    DateTimeOffset? ConfirmedAtUtc,
    string Version);

public sealed record StocktakeResponse(
    Guid Id,
    Guid? ShoppingPresetId,
    StocktakeStatus Status,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<StocktakeEntryResponse> Entries,
    string Version);
