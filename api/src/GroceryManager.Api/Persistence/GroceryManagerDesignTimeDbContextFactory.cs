using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GroceryManager.Api.Persistence;

public sealed class GroceryManagerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<GroceryManagerDbContext>
{
    public GroceryManagerDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "src", "GroceryManager.Api"))
            ? Path.Combine(Directory.GetCurrentDirectory(), "src", "GroceryManager.Api")
            : Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required for EF migrations.");
        var options = new DbContextOptionsBuilder<GroceryManagerDbContext>()
            .UseNpgsql(connection)
            .Options;
        return new GroceryManagerDbContext(options);
    }
}
