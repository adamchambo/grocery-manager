using System.ComponentModel.DataAnnotations;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreateStorageLocationRequest(
    [property: Required, StringLength(120)] string Name,
    [property: Range(0, int.MaxValue)] int SortOrder);

public sealed record UpdateStorageLocationRequest(
    [property: Required, StringLength(120)] string Name,
    [property: Range(0, int.MaxValue)] int SortOrder,
    [property: Required] string Version);

public sealed record UpdateStorageLocationOrderRequest(
    [property: Required, MinLength(1)] IReadOnlyList<Guid> StorageLocationIds);

public sealed record UpdateLocationItemOrderRequest(
    [property: Required, MinLength(1)] IReadOnlyList<Guid> PantryItemLocationIds);

public sealed record StorageLocationResponse(
    Guid Id,
    string Name,
    int SortOrder,
    bool IsDefault,
    bool IsArchived,
    string Version);
