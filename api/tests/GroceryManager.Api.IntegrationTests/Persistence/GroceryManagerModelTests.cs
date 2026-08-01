using GroceryManager.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.IntegrationTests.Persistence;

public sealed class GroceryManagerModelTests
{
    [Fact]
    public void ModelCanBeConstructed()
    {
        var options = new DbContextOptionsBuilder<GroceryManagerDbContext>()
            .UseNpgsql("Host=localhost;Database=grocery_manager_model_test;Username=test;Password=test")
            .Options;

        using var context = new GroceryManagerDbContext(options);

        Assert.NotEmpty(context.Model.GetEntityTypes());
    }
}
