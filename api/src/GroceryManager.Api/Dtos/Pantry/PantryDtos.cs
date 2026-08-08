using System.ComponentModel.DataAnnotations;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreatePantryRequest(
    [property: Required, StringLength(120)] string Name);

public sealed record UpdatePantryRequest(
    [property: Required, StringLength(120)] string Name,
    [property: Required] string Version);

public sealed record PantryResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string Version);
