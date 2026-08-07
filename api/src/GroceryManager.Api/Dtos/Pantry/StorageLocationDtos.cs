namespace GroceryManager.Api.Dtos.Pantry;

public sealed record CreateStorageLocationRequest(string Name, int SortOrder);

public sealed record UpdateStorageLocationRequest(string Name, int SortOrder, string Version);

public sealed record UpdateStorageLocationOrderRequest(IReadOnlyList<Guid> StorageLocationIds);

public sealed record UpdateLocationItemOrderRequest(IReadOnlyList<Guid> PantryItemLocationIds);

public sealed record StorageLocationResponse(
    Guid Id,
    string Name,
    int SortOrder,
    bool IsDefault,
    bool IsArchived,
    string Version);
