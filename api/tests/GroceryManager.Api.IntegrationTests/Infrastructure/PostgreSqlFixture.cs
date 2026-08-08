using GroceryManager.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace GroceryManager.Api.IntegrationTests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:17.6-alpine").Build();

    public Task InitializeAsync() => container.StartAsync();

    public Task DisposeAsync() => container.DisposeAsync().AsTask();

    public async Task<string> CreateDatabaseAsync()
    {
        var databaseName = $"grocery_manager_{Guid.NewGuid():N}";
        await using var connection = new NpgsqlConnection(container.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
        await command.ExecuteNonQueryAsync();

        var connectionString = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
        {
            Database = databaseName
        }.ConnectionString;

        await using var db = CreateContext(connectionString);
        await db.Database.MigrateAsync();
        await DefaultDataSeeder.SeedGlobalDataAsync(db);
        return connectionString;
    }

    public static GroceryManagerDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<GroceryManagerDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new GroceryManagerDbContext(options);
    }
}
