namespace GroceryManager.Modules.Entitlements.Entities;

public sealed class Plan
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
}
