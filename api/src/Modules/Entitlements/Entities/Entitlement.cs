namespace GroceryManager.Modules.Entitlements.Entities;

public sealed class Entitlement
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Description { get; set; }
}
