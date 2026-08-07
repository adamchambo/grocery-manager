using GroceryManager.Api.Enums.Shopping;

namespace GroceryManager.Api.Dtos.Shopping;

public sealed record GenerateShoppingListRequest(
    Guid? ShoppingPresetId,
    Guid? StocktakeId,
    string? Name);

public sealed record UpdateShoppingListRequest(string Name, string Version);

public sealed record AddShoppingListItemRequest(
    string Name,
    decimal SuggestedPurchaseQuantity,
    Guid? DestinationLocationId);

public sealed record UpdateShoppingListItemRequest(
    decimal? SuggestedPurchaseQuantity,
    decimal? ActualPurchaseQuantity,
    ShoppingListItemOutcome Outcome,
    Guid? DestinationLocationId,
    string Version);

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
