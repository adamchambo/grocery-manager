namespace GroceryManager.Api.Services.Identity;

public interface ICurrentUserContext
{
    public Guid? UserId { get; }
    public bool IsAuthenticated { get; }
}
