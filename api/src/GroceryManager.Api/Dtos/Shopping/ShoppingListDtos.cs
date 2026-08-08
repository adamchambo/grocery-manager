using System.ComponentModel.DataAnnotations;
using GroceryManager.Api.Enums.Shopping;

namespace GroceryManager.Api.Dtos.Shopping;

public sealed record GenerateShoppingListRequest(
    Guid? ShoppingPresetId,
    Guid? StocktakeId,
    [StringLength(160)] string? Name);

public sealed record UpdateShoppingListRequest(
    [Required, StringLength(160)] string Name,
    [Required] string Version);

public sealed record AddShoppingListItemRequest(
    [Required, StringLength(160)] string Name,
    [Range(typeof(decimal), "0.001", "999999999999999.999")] decimal SuggestedPurchaseQuantity,
    Guid? DestinationLocationId);

public sealed record UpdateShoppingListItemRequest(
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal? SuggestedPurchaseQuantity,
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal? ActualPurchaseQuantity,
    ShoppingListItemOutcome Outcome,
    Guid? DestinationLocationId,
    [Required] string Version);

public sealed record ShoppingListItemResponse(
    Guid Id,
    Guid? PantryItemId,
    Guid? DestinationLocationId,
    string ItemName,
    string? Brand,
    string? CategoryName,
    string? TrackingUnit,
    decimal? PackageSize,
    string? PackageUnit,
    decimal? StockAtGeneration,
    decimal? RequiredAtGeneration,
    decimal? SuggestedPurchaseQuantity,
    decimal? ActualPurchaseQuantity,
    ShoppingListItemOutcome Outcome,
    bool IsManual,
    int SortOrder,
    DateTimeOffset? InventoryAppliedAtUtc,
    string Version);

public sealed record ShoppingListResponse(
    Guid Id,
    Guid? SourcePresetId,
    Guid? SourceStocktakeId,
    string Name,
    ShoppingListStatus Status,
    bool StockChangedSinceGeneration,
    DateTimeOffset GeneratedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<ShoppingListItemResponse> Items,
    string Version);
