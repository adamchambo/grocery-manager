using GroceryManager.Api.Common.Exceptions;
using GroceryManager.Api.Dtos.InventoryHistory;
using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Dtos.Shopping;
using GroceryManager.Api.Dtos.Stocktakes;
using GroceryManager.Api.Entities.Identity;
using GroceryManager.Api.Enums.Pantry;
using GroceryManager.Api.Enums.Shopping;
using GroceryManager.Api.Enums.Stocktakes;
using GroceryManager.Api.IntegrationTests.Infrastructure;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services.InventoryHistory;
using GroceryManager.Api.Services.Documents;
using GroceryManager.Api.Services.Pantry;
using GroceryManager.Api.Services.Shopping;
using GroceryManager.Api.Services.Stocktakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GroceryManager.Api.IntegrationTests.Services;

public sealed class BackendFlowTests(PostgreSqlFixture fixture) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task PantryItemCannotBeReadByAnotherPantryOwner()
    {
        await using var db = await CreateContextAsync();
        var firstUserId = await CreateUserAndPantryAsync(db, "first@example.com");
        var secondUserId = await CreateUserAndPantryAsync(db, "second@example.com");
        var item = await CreateItemAsync(db, secondUserId, "Milk", TrackingUnit.Volume, null, null, 1);

        var service = new PantryItemService(db, new TestCurrentUserContext(firstUserId));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAsync(item.Id, CancellationToken.None));
    }

    [Fact]
    public async Task ShoppingGenerationCalculatesAndRoundsShortages()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "calculate@example.com");
        await CreateItemAsync(db, userId, "Bread", TrackingUnit.Package, 2, 7, 0.5m);
        await CreateItemAsync(db, userId, "Rice", TrackingUnit.Weight, 1, 7, 0.25m);
        var presetId = await GetEverythingPresetIdAsync(db, userId);

        var list = await new ShoppingListService(db, new TestCurrentUserContext(userId))
            .GenerateAsync(new GenerateShoppingListRequest(presetId, null, "Test list"), CancellationToken.None);

        Assert.Equal(2m, list.Items.Single(x => x.ItemName == "Bread").SuggestedPurchaseQuantity);
        Assert.Equal(0.75m, list.Items.Single(x => x.ItemName == "Rice").SuggestedPurchaseQuantity);
    }

    [Fact]
    public async Task ShoppingGenerationUsesCategoryThenItemNameOrderAndCustomOrderMustBeExact()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "shopping-order@example.com");
        await CreateItemAsync(db, userId, "Rice", TrackingUnit.Package, 1, 7, 0);
        await CreateItemAsync(db, userId, "Bread", TrackingUnit.Package, 1, 7, 0);
        var presetId = await GetEverythingPresetIdAsync(db, userId);
        var service = new ShoppingListService(db, new TestCurrentUserContext(userId));
        var list = await service.GenerateAsync(new GenerateShoppingListRequest(presetId, null, null), CancellationToken.None);

        Assert.Equal(["Bread", "Rice"], list.Items.Select(x => x.ItemName));
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateOrderAsync(list.Id,
            new UpdateShoppingListOrderRequest([list.Items[0].Id, list.Items[0].Id]), CancellationToken.None));
        await service.UpdateOrderAsync(list.Id, new UpdateShoppingListOrderRequest(list.Items.Select(x => x.Id).Reverse().ToArray()), CancellationToken.None);
        var reordered = await service.GetAsync(list.Id, CancellationToken.None);
        Assert.True(reordered.UsesCustomOrder);
        Assert.Equal(["Rice", "Bread"], reordered.Items.Select(x => x.ItemName));
    }

    [Fact]
    public async Task ActiveListsFlagTrackedItemsThatAlsoAppearOnAnotherList()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "duplicate-shopping@example.com");
        await CreateItemAsync(db, userId, "Bread", TrackingUnit.Package, 1, 7, 0);
        var presetId = await GetEverythingPresetIdAsync(db, userId);
        var service = new ShoppingListService(db, new TestCurrentUserContext(userId));
        var first = await service.GenerateAsync(new GenerateShoppingListRequest(presetId, null, null), CancellationToken.None);
        await service.GenerateAsync(new GenerateShoppingListRequest(presetId, null, null), CancellationToken.None);

        Assert.True((await service.GetAsync(first.Id, CancellationToken.None)).Items.Single().IsOnAnotherActiveList);
    }

    [Fact]
    public async Task ShoppingListPdfPaginatesEveryItem()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "shopping-pdf@example.com");
        var service = new ShoppingListService(db, new TestCurrentUserContext(userId));
        var list = await service.GenerateAsync(new GenerateShoppingListRequest(null, null, null), CancellationToken.None);
        for (var index = 0; index < 39; index++)
            await service.AddItemAsync(list.Id, new AddShoppingListItemRequest($"Item {index}", 1, false, null, null, null), CancellationToken.None);

        var pdf = await new ShoppingListDocumentService(db, new TestCurrentUserContext(userId)).GeneratePdfAsync(list.Id, CancellationToken.None);
        Assert.Equal(2, System.Text.Encoding.ASCII.GetString(pdf).Split("/Type /Page /Parent").Length - 1);
    }

    [Fact]
    public async Task CompletingStocktakeUpdatesInventoryAndCreatesAdjustment()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "stocktake@example.com");
        var item = await CreateItemAsync(db, userId, "Milk", TrackingUnit.Volume, null, null, 5);
        var presetId = await GetEverythingPresetIdAsync(db, userId);
        var service = new StocktakeService(db, new TestCurrentUserContext(userId));
        var stocktake = await service.StartAsync(new StartStocktakeRequest(presetId), CancellationToken.None);
        var entry = Assert.Single(stocktake.Entries);

        await service.UpdateEntryAsync(stocktake.Id, entry.Id,
            new UpdateStocktakeEntryRequest(StocktakeEntryStatus.Corrected, 2, entry.Version),
            CancellationToken.None);
        await service.CompleteAsync(stocktake.Id, null, CancellationToken.None);

        Assert.Equal(2m, await db.PantryItemLocations.Where(x => x.PantryItemId == item.Id).Select(x => x.CurrentQuantity).SingleAsync());
        var adjustment = await db.InventoryAdjustments.SingleAsync(x => x.SourceStocktakeEntryId == entry.Id);
        Assert.Equal(-3m, adjustment.QuantityDelta);
    }

    [Fact]
    public async Task ShoppingGenerationFromCompletedStocktakeUsesItsConfirmedQuantities()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "stocktake-shopping@example.com");
        var item = await CreateItemAsync(db, userId, "Milk", TrackingUnit.Package, 2, 7, 0);
        var presetId = await GetEverythingPresetIdAsync(db, userId);
        var stocktakeService = new StocktakeService(db, new TestCurrentUserContext(userId));
        var stocktake = await stocktakeService.StartAsync(new StartStocktakeRequest(presetId), CancellationToken.None);
        var entry = Assert.Single(stocktake.Entries);
        await stocktakeService.UpdateEntryAsync(stocktake.Id, entry.Id,
            new UpdateStocktakeEntryRequest(StocktakeEntryStatus.Corrected, 0, entry.Version), CancellationToken.None);
        await stocktakeService.CompleteAsync(stocktake.Id, null, CancellationToken.None);
        await new InventoryAdjustmentService(db, new TestCurrentUserContext(userId)).CreateAsync(
            new CreateInventoryAdjustmentRequest(item.Locations.Single().Id, 1, null, "after-stocktake"), CancellationToken.None);

        var list = await new ShoppingListService(db, new TestCurrentUserContext(userId))
            .GenerateAsync(new GenerateShoppingListRequest(presetId, stocktake.Id, null), CancellationToken.None);

        Assert.Equal(2m, Assert.Single(list.Items).SuggestedPurchaseQuantity);
    }

    [Fact]
    public async Task StartingSecondStocktakeWhileOneIsActiveIsRejected()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "active-stocktake@example.com");
        await CreateItemAsync(db, userId, "Milk", TrackingUnit.Volume, null, null, 1);
        var presetId = await GetEverythingPresetIdAsync(db, userId);
        var service = new StocktakeService(db, new TestCurrentUserContext(userId));

        await service.StartAsync(new StartStocktakeRequest(presetId), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.StartAsync(new StartStocktakeRequest(presetId), CancellationToken.None));
    }

    [Fact]
    public async Task PurchasingShoppingItemImmediatelyAddsInventory()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "purchase@example.com");
        var item = await CreateItemAsync(db, userId, "Bread", TrackingUnit.Package, 1, 7, 0);
        var presetId = await GetEverythingPresetIdAsync(db, userId);
        var service = new ShoppingListService(db, new TestCurrentUserContext(userId));
        var list = await service.GenerateAsync(new GenerateShoppingListRequest(presetId, null, null), CancellationToken.None);
        var listItem = Assert.Single(list.Items);

        await service.UpdateItemAsync(list.Id, listItem.Id,
            new UpdateShoppingListItemRequest(null, 3, ShoppingListItemOutcome.Purchased, item.DefaultStorageLocationId, listItem.Version),
            CancellationToken.None);

        Assert.Equal(3m, await db.PantryItemLocations.Where(x => x.PantryItemId == item.Id).Select(x => x.CurrentQuantity).SingleAsync());
        Assert.Equal(3m, (await db.InventoryAdjustments.SingleAsync(x => x.SourceShoppingListItemId == listItem.Id)).QuantityDelta);
    }

    [Fact]
    public async Task ManualTrackedItemCreatesPantryItemOnlyWhenPurchasedAndCanBeUndone()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "manual-shopping@example.com");
        var pantryId = await db.Pantries.Where(x => x.OwnerUserId == userId).Select(x => x.Id).SingleAsync();
        var categoryId = await db.Categories.Where(x => x.PantryId == pantryId).Select(x => x.Id).FirstAsync();
        var locationId = await db.StorageLocations.Where(x => x.PantryId == pantryId).Select(x => x.Id).FirstAsync();
        var service = new ShoppingListService(db, new TestCurrentUserContext(userId));
        var list = await service.GenerateAsync(new GenerateShoppingListRequest(null, null, null), CancellationToken.None);
        var manual = await service.AddItemAsync(list.Id,
            new AddShoppingListItemRequest("Tortillas", 2, true, categoryId, TrackingUnit.Package, locationId), CancellationToken.None);

        Assert.Equal(0, await db.PantryItems.CountAsync(x => x.Name == "Tortillas"));
        var purchased = await service.UpdateItemAsync(list.Id, manual.Id,
            new UpdateShoppingListItemRequest(null, 2, ShoppingListItemOutcome.Purchased, locationId, manual.Version), CancellationToken.None);
        Assert.NotNull(purchased.PantryItemId);
        Assert.Equal(2m, await db.PantryItemLocations.Where(x => x.PantryItemId == purchased.PantryItemId).Select(x => x.CurrentQuantity).SingleAsync());

        var undone = await service.UndoPurchaseAsync(list.Id, manual.Id, CancellationToken.None);
        Assert.Equal(ShoppingListItemOutcome.Pending, undone.Outcome);
        Assert.Equal(0m, await db.PantryItemLocations.Where(x => x.PantryItemId == purchased.PantryItemId).Select(x => x.CurrentQuantity).SingleAsync());
    }

    [Fact]
    public async Task ReversingAdjustmentRestoresPreviousInventory()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "reverse@example.com");
        var item = await CreateItemAsync(db, userId, "Rice", TrackingUnit.Weight, null, null, 1);
        var locationId = item.Locations.Single().Id;
        var service = new InventoryAdjustmentService(db, new TestCurrentUserContext(userId));
        var adjustment = await service.CreateAsync(
            new CreateInventoryAdjustmentRequest(locationId, 4, null, "correction-1"),
            CancellationToken.None);

        var reversal = await service.ReverseAsync(adjustment.Id,
            new ReverseInventoryAdjustmentRequest(null, "reversal-1"),
            CancellationToken.None);

        Assert.Equal(-4m, reversal.QuantityDelta);
        Assert.Equal(1m, await db.PantryItemLocations.Where(x => x.Id == locationId).Select(x => x.CurrentQuantity).SingleAsync());
    }

    [Fact]
    public async Task UndoingCompletedListCreatesCompensatingAdjustment()
    {
        await using var db = await CreateContextAsync();
        var userId = await CreateUserAndPantryAsync(db, "undo@example.com");
        var item = await CreateItemAsync(db, userId, "Bread", TrackingUnit.Package, 2, 7, 0);
        var presetId = await GetEverythingPresetIdAsync(db, userId);
        var service = new ShoppingListService(db, new TestCurrentUserContext(userId));
        var list = await service.GenerateAsync(new GenerateShoppingListRequest(presetId, null, null), CancellationToken.None);
        var listItem = Assert.Single(list.Items);
        await service.UpdateItemAsync(list.Id, listItem.Id,
            new UpdateShoppingListItemRequest(null, 2, ShoppingListItemOutcome.Purchased, item.DefaultStorageLocationId, listItem.Version),
            CancellationToken.None);
        await service.CompleteAsync(list.Id, CancellationToken.None);

        var undone = await service.UndoAsync(list.Id, CancellationToken.None);

        Assert.Equal(ShoppingListStatus.Active, undone.Status);
        Assert.Equal(0m, await db.PantryItemLocations.Where(x => x.PantryItemId == item.Id).Select(x => x.CurrentQuantity).SingleAsync());
        Assert.Equal(2, await db.InventoryAdjustments.CountAsync(x => x.PantryItemLocationId == item.Locations.Single().Id));
    }

    [Fact]
    public async Task EfConcurrencyFailureMapsToConflictResponse()
    {
        var connectionString = await fixture.CreateDatabaseAsync();
        await using var setup = PostgreSqlFixture.CreateContext(connectionString);
        var userId = await CreateUserAndPantryAsync(setup, "concurrency@example.com");
        var pantryId = await setup.Pantries.Where(x => x.OwnerUserId == userId).Select(x => x.Id).SingleAsync();
        await using var first = PostgreSqlFixture.CreateContext(connectionString);
        await using var second = PostgreSqlFixture.CreateContext(connectionString);
        var firstPantry = await first.Pantries.SingleAsync(x => x.Id == pantryId);
        var secondPantry = await second.Pantries.SingleAsync(x => x.Id == pantryId);
        firstPantry.Name = "First update";
        await first.SaveChangesAsync();
        secondPantry.Name = "Second update";
        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        var details = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(details, NullLogger<ApiExceptionHandler>.Instance);
        var context = new DefaultHttpContext();

        Assert.True(await handler.TryHandleAsync(context, exception, CancellationToken.None));
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, details.Context?.ProblemDetails.Status);
    }

    private async Task<GroceryManagerDbContext> CreateContextAsync() =>
        PostgreSqlFixture.CreateContext(await fixture.CreateDatabaseAsync());

    private static async Task<Guid> CreateUserAndPantryAsync(GroceryManagerDbContext db, string email)
    {
        var userId = Guid.NewGuid();
        db.Users.Add(new ApplicationUser { Id = userId, UserName = email, NormalizedUserName = email.ToUpperInvariant(), Email = email, NormalizedEmail = email.ToUpperInvariant() });
        await db.SaveChangesAsync();
        await new PantryService(db, new TestCurrentUserContext(userId))
            .CreateAsync(new CreatePantryRequest("My pantry"), CancellationToken.None);
        return userId;
    }

    private static async Task<PantryItemResponse> CreateItemAsync(
        GroceryManagerDbContext db, Guid userId, string name, TrackingUnit unit,
        decimal? consumptionQuantity, decimal? consumptionPeriodDays, decimal currentQuantity)
    {
        var pantryId = await db.Pantries.Where(x => x.OwnerUserId == userId).Select(x => x.Id).SingleAsync();
        var categoryId = await db.Categories.Where(x => x.PantryId == pantryId).Select(x => x.Id).FirstAsync();
        var locationId = await db.StorageLocations.Where(x => x.PantryId == pantryId).OrderBy(x => x.SortOrder).Select(x => x.Id).FirstAsync();
        return await new PantryItemService(db, new TestCurrentUserContext(userId)).CreateAsync(
            new CreatePantryItemRequest(categoryId, null, locationId, name, null, null, null, null, unit, null, null,
                consumptionQuantity, consumptionPeriodDays, 0,
                [new PantryItemLocationRequest(locationId, currentQuantity, 0)]),
            CancellationToken.None);
    }

    private static async Task<Guid> GetEverythingPresetIdAsync(GroceryManagerDbContext db, Guid userId)
    {
        var pantryId = await db.Pantries.Where(x => x.OwnerUserId == userId).Select(x => x.Id).SingleAsync();
        return await db.ShoppingPresets.Where(x => x.PantryId == pantryId && x.IsEverythingPreset).Select(x => x.Id).SingleAsync();
    }

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? Context { get; private set; }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.FromResult(true);
        }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.CompletedTask;
        }
    }
}
