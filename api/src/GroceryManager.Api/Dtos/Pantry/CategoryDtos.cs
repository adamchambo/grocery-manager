namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreateCategoryRequest(string Name, int SortOrder);

public sealed record UpdateCategoryRequest(string Name, int SortOrder, string Version);

public sealed record UpdateCategoryOrderRequest(IReadOnlyList<Guid> CategoryIds);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    int SortOrder,
    bool IsDefault,
    bool IsArchived,
    string Version);
