namespace GroceryManager.Api.Services.Identity;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}
