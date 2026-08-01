namespace GroceryManager.Modules.ShoppingPresets.Entities;

public sealed class PresetItemRule
{
    public Guid ShoppingPresetId { get; set; }
    public Guid PantryItemId { get; set; }
    public PresetItemRuleType RuleType { get; set; }
}
