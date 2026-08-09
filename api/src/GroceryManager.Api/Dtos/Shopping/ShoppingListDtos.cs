using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GroceryManager.Api.Enums.Pantry;
using GroceryManager.Api.Enums.Shopping;

namespace GroceryManager.Api.Dtos.Shopping;

public sealed record GenerateShoppingListRequest(
    Guid? StocktakeId,
    [StringLength(160)] string? Name)
{
    [Obsolete("Shopping presets are no longer used. Generate a list from a stocktake.")]
    public GenerateShoppingListRequest(Guid? _, Guid? stocktakeId, string? name) : this(stocktakeId, name) { }
}

public sealed record UpdateShoppingListRequest(
    [Required, StringLength(160)] string Name,
    [Required] string Version);

public sealed record AddShoppingListItemRequest(
    [Required, StringLength(160)] string Name,
    [Range(typeof(decimal), "0.001", "999999999999999.999")] decimal SuggestedPurchaseQuantity)
{
    [Obsolete("One-off items are list-only and need no Pantry setup.")]
    public AddShoppingListItemRequest(string name, decimal suggestedPurchaseQuantity, bool createPantryItemOnPurchase, Guid? pantryCategoryId, TrackingUnit? pantryTrackingUnit, Guid? destinationLocationId)
        : this(name, suggestedPurchaseQuantity) { }
}

public sealed record UpdateShoppingListOrderRequest(
    [Required, MinLength(1)] IReadOnlyList<Guid> ShoppingListItemIds);

public sealed record UpdateShoppingListItemRequest(
    [Range(typeof(decimal), "0", "999999999999999.999")] decimal? SuggestedPurchaseQuantity,
    ShoppingListItemOutcome Outcome,
    [Required] string Version)
{
    [Obsolete("Actual purchases and destinations are no longer tracked.")]
    public UpdateShoppingListItemRequest(decimal? suggestedPurchaseQuantity, decimal? actualPurchaseQuantity, ShoppingListItemOutcome outcome, Guid? destinationLocationId, string version)
        : this(suggestedPurchaseQuantity, outcome, version) { }
}

public sealed record ShoppingListItemResponse(
    Guid Id,
    Guid? PantryItemId,
    string ItemName,
    string? CategoryName,
    string? TrackingUnit,
    decimal? PackageSize,
    string? PackageUnit,
    decimal? StockAtGeneration,
    decimal? RequiredAtGeneration,
    decimal? SuggestedPurchaseQuantity,
    ShoppingListItemOutcome Outcome,
    bool IsManual,
    int SortOrder,
    string Version)
{
    [Obsolete("Concurrent-list warnings are no longer part of the shopping workflow.")]
    [JsonIgnore] public bool IsOnAnotherActiveList => false;
}

public sealed record ShoppingListResponse(
    Guid Id,
    Guid? SourceStocktakeId,
    string Name,
    ShoppingListStatus Status,
    DateTimeOffset GeneratedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<ShoppingListItemResponse> Items,
    string Version)
{
    [Obsolete("Manual list ordering is no longer part of the shopping workflow.")]
    [JsonIgnore] public bool UsesCustomOrder => false;
    [Obsolete("Running inventory is no longer part of the shopping workflow.")]
    [JsonIgnore] public bool StockChangedSinceGeneration => false;
}
