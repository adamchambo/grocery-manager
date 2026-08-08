using System.ComponentModel.DataAnnotations;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreateCategoryRequest(
    [Required, StringLength(120)] string Name,
    [Range(0, int.MaxValue)] int SortOrder);

public sealed record UpdateCategoryRequest(
    [Required, StringLength(120)] string Name,
    [Range(0, int.MaxValue)] int SortOrder,
    [Required] string Version);

public sealed record UpdateCategoryOrderRequest(
    [Required, MinLength(1)] IReadOnlyList<Guid> CategoryIds);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    int SortOrder,
    bool IsDefault,
    bool IsArchived,
    string Version);
