using System.ComponentModel.DataAnnotations;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreateStorageLocationRequest(
    [Required, StringLength(120)] string Name,
    [Range(0, int.MaxValue)] int SortOrder);

public sealed record UpdateStorageLocationRequest(
    [Required, StringLength(120)] string Name,
    [Range(0, int.MaxValue)] int SortOrder,
    [Required] string Version);

public sealed record UpdateStorageLocationOrderRequest(
    [Required, MinLength(1)] IReadOnlyList<Guid> StorageLocationIds);

public sealed record UpdateLocationItemOrderRequest(
    [Required, MinLength(1)] IReadOnlyList<Guid> PantryItemLocationIds);

public sealed record StorageLocationResponse(
    Guid Id,
    string Name,
    int SortOrder,
    bool IsDefault,
    bool IsArchived,
    string Version);
