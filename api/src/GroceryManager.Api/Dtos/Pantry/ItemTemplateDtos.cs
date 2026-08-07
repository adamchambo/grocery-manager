using GroceryManager.Api.Enums.Pantry;

namespace GroceryManager.Api.Dtos.Pantry;

public sealed record ItemTemplateResponse(
    Guid Id,
    string Name,
    string DefaultCategoryKey,
    TrackingUnit DefaultTrackingUnit,
    int SortOrder);
