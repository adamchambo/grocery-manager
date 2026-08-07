namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreatePantryRequest(string Name);

public sealed record UpdatePantryRequest(string Name, string Version);

public sealed record PantryResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string Version);
