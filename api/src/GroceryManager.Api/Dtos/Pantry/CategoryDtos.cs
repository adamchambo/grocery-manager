using System.ComponentModel.DataAnnotations;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreateCategoryRequest(
    [property: Required, StringLength(120)] string Name,
    [property: Range(0, int.MaxValue)] int SortOrder);

public sealed record UpdateCategoryRequest(
    [property: Required, StringLength(120)] string Name,
    [property: Range(0, int.MaxValue)] int SortOrder,
    [property: Required] string Version);

public sealed record UpdateCategoryOrderRequest(
    [property: Required, MinLength(1)] IReadOnlyList<Guid> CategoryIds);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    int SortOrder,
    bool IsDefault,
    bool IsArchived,
    string Version);
