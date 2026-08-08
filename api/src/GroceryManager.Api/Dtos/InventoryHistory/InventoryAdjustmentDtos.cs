using System.ComponentModel.DataAnnotations;
using GroceryManager.Api.Enums.InventoryHistory;

namespace GroceryManager.Api.Dtos.InventoryHistory;

public sealed record CreateInventoryAdjustmentRequest(
    Guid PantryItemLocationId,
    decimal QuantityDelta,
    [property: StringLength(2000)] string? Notes,
    [property: Required, StringLength(200)] string IdempotencyKey) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (QuantityDelta == 0)
            yield return new ValidationResult("Quantity delta cannot be zero.", [nameof(QuantityDelta)]);
    }
}

public sealed record ReverseInventoryAdjustmentRequest(
    [property: StringLength(2000)] string? Notes,
    [property: Required, StringLength(200)] string IdempotencyKey);

public sealed record InventoryAdjustmentResponse(
    Guid Id,
    Guid PantryItemLocationId,
    Guid? SourceStocktakeEntryId,
    Guid? SourceShoppingListItemId,
    Guid? ReversesAdjustmentId,
    InventoryAdjustmentType AdjustmentType,
    decimal QuantityDelta,
    string? Notes,
    DateTimeOffset CreatedAtUtc);
