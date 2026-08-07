namespace GroceryManager.Api.Entities.Entitlements;

public sealed class PlanEntitlement
{
    public Guid PlanId { get; set; }
    public Guid EntitlementId { get; set; }
    public decimal? LimitValue { get; set; }
}
