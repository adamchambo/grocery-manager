using GroceryManager.Api.Enums.ShoppingPresets;

namespace GroceryManager.Api.Dtos.ShoppingPresets;

public sealed record ShoppingPresetItemRuleRequest(Guid PantryItemId, PresetItemRuleType RuleType);

public sealed record CreateShoppingPresetRequest(
    string Name,
    decimal CoverageDays,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<ShoppingPresetItemRuleRequest> ItemRules);

public sealed record UpdateShoppingPresetRequest(
    string Name,
    decimal CoverageDays,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<ShoppingPresetItemRuleRequest> ItemRules,
    string Version);

public sealed record ShoppingPresetResponse(
    Guid Id,
    string Name,
    decimal CoverageDays,
    bool IsEverythingPreset,
    bool IsArchived,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<ShoppingPresetItemRuleRequest> ItemRules,
    string Version);

public sealed record ShoppingPresetPreviewItemResponse(
    Guid PantryItemId,
    string Name,
    bool Included,
    string InclusionReason);

public sealed record ShoppingPresetPreviewResponse(
    Guid ShoppingPresetId,
    IReadOnlyList<ShoppingPresetPreviewItemResponse> Items);
