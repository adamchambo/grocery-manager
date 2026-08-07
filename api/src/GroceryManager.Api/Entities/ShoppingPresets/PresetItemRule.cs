using GroceryManager.Api.Enums.ShoppingPresets;

namespace GroceryManager.Api.Entities.ShoppingPresets;

public sealed class PresetItemRule
{
    public Guid ShoppingPresetId { get; set; }
    public Guid PantryItemId { get; set; }
    public PresetItemRuleType RuleType { get; set; }
}
