using GroceryManager.Api.Services.Identity;

namespace GroceryManager.Api.IntegrationTests.Infrastructure;

internal sealed class TestCurrentUserContext(Guid userId) : ICurrentUserContext
{
    public Guid? UserId => userId;

    public bool IsAuthenticated => true;
}
