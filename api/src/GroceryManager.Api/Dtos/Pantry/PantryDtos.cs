using System.ComponentModel.DataAnnotations;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreatePantryRequest(
    [Required, StringLength(120)] string Name);

public sealed record UpdatePantryRequest(
    [Required, StringLength(120)] string Name,
    [Required] string Version);

public sealed record PantryResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string Version);
