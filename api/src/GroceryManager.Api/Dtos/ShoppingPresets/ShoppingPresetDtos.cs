using System.ComponentModel.DataAnnotations;
using GroceryManager.Api.Enums.ShoppingPresets;

namespace GroceryManager.Api.Dtos.ShoppingPresets;

public sealed record ShoppingPresetItemRuleRequest(Guid PantryItemId, PresetItemRuleType RuleType);

public sealed record CreateShoppingPresetRequest(
    [property: Required, StringLength(120)] string Name,
    [property: Range(typeof(decimal), "0.001", "999999999999999.999")] decimal CoverageDays,
    [property: Required] IReadOnlyList<Guid> CategoryIds,
    [property: Required] IReadOnlyList<ShoppingPresetItemRuleRequest> ItemRules);

public sealed record UpdateShoppingPresetRequest(
    [property: Required, StringLength(120)] string Name,
    [property: Range(typeof(decimal), "0.001", "999999999999999.999")] decimal CoverageDays,
    [property: Required] IReadOnlyList<Guid> CategoryIds,
    [property: Required] IReadOnlyList<ShoppingPresetItemRuleRequest> ItemRules,
    [property: Required] string Version);

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
