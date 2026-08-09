using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Common.Exceptions;
using GroceryManager.Api.Dtos.Shopping;
using GroceryManager.Api.Entities.InventoryHistory;
using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Entities.Shopping;
using GroceryManager.Api.Enums.InventoryHistory;
using GroceryManager.Api.Enums.Pantry;
using GroceryManager.Api.Enums.Shopping;
using GroceryManager.Api.Enums.ShoppingPresets;
using GroceryManager.Api.Enums.Stocktakes;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.Shopping;

public sealed class ShoppingListService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : IShoppingListService
{
    public async Task<PagedResponse<ShoppingListResponse>> ListAsync(int page, int pageSize, ShoppingListStatus? status, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        (page, pageSize) = ServiceSupport.NormalizePage(page, pageSize);
        var query = db.ShoppingLists.AsNoTracking().Where(x => x.PantryId == pantryId);
        if (status is not null) query = query.Where(x => x.Status == status);
        var total = await query.CountAsync(cancellationToken);
        var lists = await query.OrderByDescending(x => x.GeneratedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new(await MapManyAsync(lists, cancellationToken), page, pageSize, total);
    }

    public async Task<ShoppingListResponse> GetAsync(Guid listId, CancellationToken cancellationToken) =>
        await MapAsync(await FindAsync(listId, cancellationToken), cancellationToken);

    public async Task<ShoppingListResponse> GenerateAsync(GenerateShoppingListRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        var routineIntervalDays = await db.Pantries.Where(x => x.Id == pantryId)
            .Select(x => x.ShoppingIntervalDays).SingleAsync(cancellationToken);
        var preset = request.ShoppingPresetId is Guid presetId
            ? await db.ShoppingPresets.SingleOrDefaultAsync(x => x.Id == presetId && x.PantryId == pantryId && !x.IsArchived, cancellationToken)
                ?? throw new ArgumentException("The shopping preset is invalid.")
            : null;
        var stocktake = request.StocktakeId is Guid stocktakeId
            ? await db.Stocktakes.SingleOrDefaultAsync(x => x.Id == stocktakeId && x.PantryId == pantryId && x.Status == StocktakeStatus.Completed, cancellationToken)
                ?? throw new ArgumentException("The stocktake must exist and be completed.")
            : null;
        if (stocktake is not null && preset is not null && stocktake.ShoppingPresetId != preset.Id)
            throw new ArgumentException("The stocktake was not performed for this preset.");
        if (stocktake is not null && await db.ShoppingLists.AnyAsync(x => x.SourceStocktakeId == stocktake.Id, cancellationToken))
            throw new ConflictException("This stocktake has already generated a shopping list.");

        var pantryItems = await GetPresetItemsAsync(pantryId, preset?.Id, cancellationToken);
        var categories = await db.Categories.AsNoTracking().Where(x => x.PantryId == pantryId).ToDictionaryAsync(x => x.Id, cancellationToken);
        var itemIds = pantryItems.Select(x => x.Id).ToArray();
        var locations = await db.PantryItemLocations.AsNoTracking().Where(x => itemIds.Contains(x.PantryItemId)).ToListAsync(cancellationToken);
        var stocktakeQuantities = stocktake is null
            ? null
            : await db.StocktakeEntries.AsNoTracking().Where(x => x.StocktakeId == stocktake.Id)
                .ToDictionaryAsync(x => x.PantryItemLocationId, x => x.RecordedQuantity ?? x.PreviousConfirmedQuantity, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var list = new ShoppingList
        {
            Id = Guid.NewGuid(), PantryId = pantryId, SourcePresetId = preset?.Id, SourceStocktakeId = stocktake?.Id,
            Name = string.IsNullOrWhiteSpace(request.Name) ? $"Shopping list {now:d MMM yyyy}" : request.Name.Trim(),
            Status = ShoppingListStatus.Active, GeneratedAtUtc = now
        };
        var generatedItems = pantryItems.Select(item =>
        {
            var stock = locations.Where(x => x.PantryItemId == item.Id)
                .Sum(x => stocktakeQuantities?.GetValueOrDefault(x.Id, x.CurrentQuantity) ?? x.CurrentQuantity);
            var required = CalculateRequired(item, routineIntervalDays);
            var purchase = RoundPurchase(item.TrackingUnit, Math.Max(0, required - stock));
            return (Item: item, Stock: stock, Required: required, Purchase: purchase);
        }).Where(x => x.Purchase > 0).OrderBy(x => categories[x.Item.CategoryId].Name).ThenBy(x => x.Item.Name).Select((x, index) => new ShoppingListItem
        {
            Id = Guid.NewGuid(), ShoppingListId = list.Id, PantryItemId = x.Item.Id,
            DestinationLocationId = x.Item.DefaultStorageLocationId, ItemNameSnapshot = x.Item.Name,
            BrandSnapshot = x.Item.Brand, CategoryNameSnapshot = categories[x.Item.CategoryId].Name,
            TrackingUnitSnapshot = x.Item.TrackingUnit.ToString(), PackageSizeSnapshot = x.Item.PackageSize,
            PackageUnitSnapshot = x.Item.PackageUnit, StockAtGeneration = x.Stock, RequiredAtGeneration = x.Required,
            SuggestedPurchaseQuantity = x.Purchase, Outcome = ShoppingListItemOutcome.Pending, SortOrder = index
        }).ToList();
        db.ShoppingLists.Add(list);
        db.ShoppingListItems.AddRange(generatedItems);
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(list, cancellationToken);
    }

    public async Task<ShoppingListResponse> UpdateAsync(Guid listId, UpdateShoppingListRequest request, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        EnsureActive(list);
        ServiceSupport.ApplyVersion(db, list, request.Version);
        list.Name = request.Name.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(list, cancellationToken);
    }

    public async Task<ShoppingListItemResponse> AddItemAsync(Guid listId, AddShoppingListItemRequest request, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        EnsureActive(list);
        if (request.SuggestedPurchaseQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(request));
        if (request.DestinationLocationId is not null && !await db.StorageLocations.AnyAsync(x => x.Id == request.DestinationLocationId && x.PantryId == list.PantryId && !x.IsArchived, cancellationToken))
            throw new ArgumentException("The destination location is invalid.");
        Category? category = null;
        if (request.CreatePantryItemOnPurchase)
        {
            if (request.PantryCategoryId is null || request.PantryTrackingUnit is null || request.DestinationLocationId is null)
                throw new ArgumentException("Tracked manual items require a category, tracking unit, and destination location.");
            category = await db.Categories.SingleOrDefaultAsync(x => x.Id == request.PantryCategoryId && x.PantryId == list.PantryId && !x.IsArchived, cancellationToken)
                ?? throw new ArgumentException("The pantry category is invalid.");
        }
        var sortOrder = await db.ShoppingListItems.Where(x => x.ShoppingListId == list.Id).Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? -1;
        var item = new ShoppingListItem
        {
            Id = Guid.NewGuid(), ShoppingListId = list.Id, ItemNameSnapshot = request.Name.Trim(),
            SuggestedPurchaseQuantity = request.SuggestedPurchaseQuantity, DestinationLocationId = request.DestinationLocationId,
            PantryCategoryId = category?.Id, CategoryNameSnapshot = category?.Name, PantryTrackingUnit = request.PantryTrackingUnit,
            TrackingUnitSnapshot = request.PantryTrackingUnit?.ToString(), CreatePantryItemOnPurchase = request.CreatePantryItemOnPurchase,
            Outcome = ShoppingListItemOutcome.Pending, IsManual = true, SortOrder = sortOrder + 1
        };
        db.ShoppingListItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return (await MapAsync(list, cancellationToken)).Items.Single(x => x.Id == item.Id);
    }

    public async Task<ShoppingListItemResponse> UpdateItemAsync(Guid listId, Guid itemId, UpdateShoppingListItemRequest request, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        EnsureActive(list);
        var item = await db.ShoppingListItems.SingleOrDefaultAsync(x => x.Id == itemId && x.ShoppingListId == list.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Shopping list item not found.");
        ServiceSupport.ApplyVersion(db, item, request.Version);
        if (request.SuggestedPurchaseQuantity < 0 || request.ActualPurchaseQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(request), "Purchase quantities cannot be negative.");
        if (item.InventoryAppliedAtUtc is not null && request.Outcome != item.Outcome)
            throw new ConflictException("Undo the applied purchase before changing its outcome.");
        if (request.DestinationLocationId is not null && !await db.StorageLocations.AnyAsync(x => x.Id == request.DestinationLocationId && x.PantryId == list.PantryId && !x.IsArchived, cancellationToken))
            throw new ArgumentException("The destination location is invalid.");

        item.SuggestedPurchaseQuantity = request.SuggestedPurchaseQuantity ?? item.SuggestedPurchaseQuantity;
        item.ActualPurchaseQuantity = request.Outcome == ShoppingListItemOutcome.NotPurchased ? 0 : request.ActualPurchaseQuantity;
        item.DestinationLocationId = request.DestinationLocationId ?? item.DestinationLocationId;
        item.Outcome = request.Outcome;
        if (request.Outcome is ShoppingListItemOutcome.Purchased or ShoppingListItemOutcome.PartiallyPurchased && item.InventoryAppliedAtUtc is null)
            await ApplyPurchaseAsync(list, item, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return (await MapAsync(list, cancellationToken)).Items.Single(x => x.Id == item.Id);
    }

    public async Task UpdateOrderAsync(Guid listId, UpdateShoppingListOrderRequest request, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        EnsureActive(list);
        var items = await db.ShoppingListItems.Where(x => x.ShoppingListId == list.Id).ToDictionaryAsync(x => x.Id, cancellationToken);
        if (items.Count != request.ShoppingListItemIds.Count || items.Count != request.ShoppingListItemIds.Distinct().Count() || request.ShoppingListItemIds.Any(id => !items.ContainsKey(id)))
            throw new ArgumentException("The shopping list order must include every item exactly once.");
        for (var index = 0; index < request.ShoppingListItemIds.Count; index++) items[request.ShoppingListItemIds[index]].SortOrder = index;
        list.UsesCustomOrder = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        EnsureActive(list);
        var item = await db.ShoppingListItems.SingleOrDefaultAsync(x => x.Id == itemId && x.ShoppingListId == list.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Shopping list item not found.");
        if (item.InventoryAppliedAtUtc is not null) throw new ConflictException("An item with applied inventory cannot be removed.");
        db.ShoppingListItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShoppingListItemResponse> UndoPurchaseAsync(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        EnsureActive(list);
        var item = await db.ShoppingListItems.SingleOrDefaultAsync(x => x.Id == itemId && x.ShoppingListId == list.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Shopping list item not found.");
        if (item.InventoryAppliedAtUtc is null) throw new ConflictException("This purchase has not been applied.");
        var adjustment = await db.InventoryAdjustments.SingleAsync(x => x.SourceShoppingListItemId == item.Id, cancellationToken);
        if (await db.InventoryAdjustments.AnyAsync(x => x.ReversesAdjustmentId == adjustment.Id, cancellationToken))
            throw new ConflictException("This purchase has already been undone.");
        var location = await db.PantryItemLocations.SingleAsync(x => x.Id == adjustment.PantryItemLocationId, cancellationToken);
        if (location.CurrentQuantity - adjustment.QuantityDelta < 0) throw new ConflictException("Undo would make inventory negative.");
        var now = DateTimeOffset.UtcNow;
        location.CurrentQuantity -= adjustment.QuantityDelta; location.UpdatedAtUtc = now;
        db.InventoryAdjustments.Add(new InventoryAdjustment
        {
            Id = Guid.NewGuid(), PantryItemLocationId = location.Id, ReversesAdjustmentId = adjustment.Id,
            CreatedByUserId = ServiceSupport.RequireUserId(currentUser), AdjustmentType = InventoryAdjustmentType.Reversal,
            QuantityDelta = -adjustment.QuantityDelta, IdempotencyKey = $"shopping-item-undo:{adjustment.Id}", CreatedAtUtc = now
        });
        item.InventoryAppliedAtUtc = null; item.ActualPurchaseQuantity = null; item.Outcome = ShoppingListItemOutcome.Pending;
        list.StockChangedSinceGeneration = true;
        await db.SaveChangesAsync(cancellationToken);
        return (await MapAsync(list, cancellationToken)).Items.Single(x => x.Id == item.Id);
    }

    public async Task<ShoppingListResponse> RecalculateAsync(Guid listId, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        EnsureActive(list);
        var routineIntervalDays = await db.Pantries.Where(x => x.Id == list.PantryId)
            .Select(x => x.ShoppingIntervalDays).SingleAsync(cancellationToken);
        var rows = await (from listItem in db.ShoppingListItems
                          join pantryItem in db.PantryItems on listItem.PantryItemId equals pantryItem.Id
                          where listItem.ShoppingListId == list.Id && listItem.Outcome == ShoppingListItemOutcome.Pending
                          select new { ListItem = listItem, PantryItem = pantryItem }).ToListAsync(cancellationToken);
        var ids = rows.Select(x => x.PantryItem.Id).ToArray();
        var stocks = await db.PantryItemLocations.Where(x => ids.Contains(x.PantryItemId)).GroupBy(x => x.PantryItemId)
            .ToDictionaryAsync(x => x.Key, x => x.Sum(y => y.CurrentQuantity), cancellationToken);
        foreach (var row in rows)
        {
            var stock = stocks.GetValueOrDefault(row.PantryItem.Id);
            var required = CalculateRequired(row.PantryItem, routineIntervalDays);
            row.ListItem.StockAtGeneration = stock; row.ListItem.RequiredAtGeneration = required;
            row.ListItem.SuggestedPurchaseQuantity = RoundPurchase(row.PantryItem.TrackingUnit, Math.Max(0, required - stock));
        }
        list.StockChangedSinceGeneration = false;
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(list, cancellationToken);
    }

    public async Task<ShoppingListResponse> CompleteAsync(Guid listId, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        EnsureActive(list);
        if (await db.ShoppingListItems.AnyAsync(x => x.ShoppingListId == list.Id && x.Outcome == ShoppingListItemOutcome.Pending, cancellationToken))
            throw new ConflictException("Every shopping-list item requires an outcome before completion.");
        list.Status = ShoppingListStatus.Completed; list.CompletedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(list, cancellationToken);
    }

    public async Task<ShoppingListResponse> UndoAsync(Guid listId, CancellationToken cancellationToken)
    {
        var list = await FindAsync(listId, cancellationToken);
        if (list.Status != ShoppingListStatus.Completed) throw new ConflictException("Only a completed shopping list can be undone.");
        var items = await db.ShoppingListItems.Where(x => x.ShoppingListId == list.Id && x.InventoryAppliedAtUtc != null).ToListAsync(cancellationToken);
        var itemIds = items.Select(x => x.Id).ToArray();
        var adjustments = await db.InventoryAdjustments.Where(x => x.SourceShoppingListItemId != null && itemIds.Contains(x.SourceShoppingListItemId.Value)).ToListAsync(cancellationToken);
        var locationIds = adjustments.Select(x => x.PantryItemLocationId).Distinct().ToArray();
        var locations = await db.PantryItemLocations.Where(x => locationIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        foreach (var adjustment in adjustments)
        {
            if (await db.InventoryAdjustments.AnyAsync(x => x.ReversesAdjustmentId == adjustment.Id, cancellationToken)) continue;
            var location = locations[adjustment.PantryItemLocationId];
            if (location.CurrentQuantity - adjustment.QuantityDelta < 0) throw new ConflictException("Undo would make inventory negative.");
            location.CurrentQuantity -= adjustment.QuantityDelta; location.UpdatedAtUtc = now;
            db.InventoryAdjustments.Add(new InventoryAdjustment
            {
                Id = Guid.NewGuid(), PantryItemLocationId = location.Id, ReversesAdjustmentId = adjustment.Id,
                CreatedByUserId = ServiceSupport.RequireUserId(currentUser), AdjustmentType = InventoryAdjustmentType.Reversal,
                QuantityDelta = -adjustment.QuantityDelta, IdempotencyKey = $"shopping-undo:{adjustment.Id}", CreatedAtUtc = now
            });
        }
        foreach (var item in items) { item.InventoryAppliedAtUtc = null; item.ActualPurchaseQuantity = null; item.Outcome = ShoppingListItemOutcome.Pending; }
        list.Status = ShoppingListStatus.Active; list.CompletedAtUtc = null; list.StockChangedSinceGeneration = true;
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(list, cancellationToken);
    }

    private async Task ApplyPurchaseAsync(ShoppingList list, ShoppingListItem item, CancellationToken cancellationToken)
    {
        var quantity = item.ActualPurchaseQuantity ?? item.SuggestedPurchaseQuantity ?? 0;
        if (quantity <= 0) throw new ArgumentException("A purchased item requires a positive actual quantity.");
        PantryItemLocation? location = null;
        if (item.PantryItemId is null && item.CreatePantryItemOnPurchase)
            location = await CreatePantryItemForManualPurchaseAsync(list, item, cancellationToken);
        else if (item.PantryItemId is Guid pantryItemId && item.DestinationLocationId is Guid locationId)
            location = await db.PantryItemLocations.SingleOrDefaultAsync(x => x.PantryItemId == pantryItemId && x.StorageLocationId == locationId, cancellationToken);
        else if (item.PantryItemId is null)
            return;
        if (location is null) throw new ConflictException("A tracked purchase requires a valid destination location assigned to the pantry item.");
        var now = DateTimeOffset.UtcNow;
        location.CurrentQuantity += quantity; location.UpdatedAtUtc = now;
        item.ActualPurchaseQuantity = quantity; item.InventoryAppliedAtUtc = now;
        db.InventoryAdjustments.Add(new InventoryAdjustment
        {
            Id = Guid.NewGuid(), PantryItemLocationId = location.Id, SourceShoppingListItemId = item.Id,
            CreatedByUserId = ServiceSupport.RequireUserId(currentUser), AdjustmentType = InventoryAdjustmentType.Purchase,
            QuantityDelta = quantity, IdempotencyKey = $"shopping-item:{item.Id}", CreatedAtUtc = now
        });
        var otherLists = await (from otherList in db.ShoppingLists
                                join otherItem in db.ShoppingListItems on otherList.Id equals otherItem.ShoppingListId
                                where otherList.Id != list.Id && otherList.Status == ShoppingListStatus.Active && otherItem.PantryItemId == item.PantryItemId
                                select otherList).Distinct().ToListAsync(cancellationToken);
        foreach (var other in otherLists) other.StockChangedSinceGeneration = true;
    }

    private async Task<PantryItemLocation> CreatePantryItemForManualPurchaseAsync(ShoppingList list, ShoppingListItem item, CancellationToken cancellationToken)
    {
        if (item.PantryCategoryId is null || item.PantryTrackingUnit is null || item.DestinationLocationId is null)
            throw new ConflictException("This manual item is missing its Pantry setup.");
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == item.PantryCategoryId && x.PantryId == list.PantryId && !x.IsArchived, cancellationToken)
            ?? throw new ConflictException("The selected Pantry category is no longer available.");
        var storage = await db.StorageLocations.SingleOrDefaultAsync(x => x.Id == item.DestinationLocationId && x.PantryId == list.PantryId && !x.IsArchived, cancellationToken)
            ?? throw new ConflictException("The selected Pantry location is no longer available.");
        var now = DateTimeOffset.UtcNow;
        var pantryItem = new PantryItem
        {
            Id = Guid.NewGuid(), PantryId = list.PantryId, CategoryId = category.Id, DefaultStorageLocationId = storage.Id,
            Name = item.ItemNameSnapshot, TrackingUnit = item.PantryTrackingUnit.Value, BufferDays = 0,
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        var sortOrder = (await db.PantryItemLocations.Where(x => x.StorageLocationId == storage.Id).Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? -1) + 1;
        var location = new PantryItemLocation
        {
            Id = Guid.NewGuid(), PantryItemId = pantryItem.Id, StorageLocationId = storage.Id,
            SortOrder = sortOrder, CurrentQuantity = 0, UpdatedAtUtc = now
        };
        item.PantryItemId = pantryItem.Id;
        db.AddRange(pantryItem, location);
        return location;
    }

    private async Task<List<PantryItem>> GetPresetItemsAsync(Guid pantryId, Guid? presetId, CancellationToken cancellationToken)
    {
        var query = db.PantryItems.Where(x => x.PantryId == pantryId && !x.IsArchived);
        if (presetId is null) return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var preset = await db.ShoppingPresets.SingleAsync(x => x.Id == presetId, cancellationToken);
        if (preset.IsEverythingPreset) return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var categories = db.PresetCategories.Where(x => x.ShoppingPresetId == presetId).Select(x => x.CategoryId);
        var includes = db.PresetItemRules.Where(x => x.ShoppingPresetId == presetId && x.RuleType == PresetItemRuleType.Include).Select(x => x.PantryItemId);
        var excludes = db.PresetItemRules.Where(x => x.ShoppingPresetId == presetId && x.RuleType == PresetItemRuleType.Exclude).Select(x => x.PantryItemId);
        return await query.Where(x => (categories.Contains(x.CategoryId) || includes.Contains(x.Id)) && !excludes.Contains(x.Id)).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    private async Task<ShoppingList> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await db.ShoppingLists.SingleOrDefaultAsync(x => x.Id == id && x.PantryId == pantryId, cancellationToken)
            ?? throw new KeyNotFoundException("Shopping list not found.");
    }

    private static decimal CalculateRequired(PantryItem item, decimal coverageDays) =>
        item.ConsumptionQuantity is decimal quantity && item.ConsumptionPeriodDays is decimal period
            ? quantity / period * (coverageDays + item.BufferDays) : 0;

    private static decimal RoundPurchase(TrackingUnit unit, decimal quantity) =>
        unit is TrackingUnit.Package or TrackingUnit.Item ? Math.Ceiling(quantity) : decimal.Round(quantity, 3, MidpointRounding.AwayFromZero);

    private static void EnsureActive(ShoppingList list)
    {
        if (list.Status != ShoppingListStatus.Active) throw new ConflictException("The shopping list is not active.");
    }

    private async Task<IReadOnlyList<ShoppingListResponse>> MapManyAsync(IReadOnlyList<ShoppingList> lists, CancellationToken cancellationToken)
    {
        var ids = lists.Select(x => x.Id).ToArray();
        var items = await db.ShoppingListItems.AsNoTracking().Where(x => ids.Contains(x.ShoppingListId)).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var pantryId = lists.FirstOrDefault()?.PantryId;
        HashSet<Guid> duplicateItemIds = [];
        if (pantryId is Guid value)
        {
            var activeItems = await (from item in db.ShoppingListItems.AsNoTracking()
                                     join list in db.ShoppingLists.AsNoTracking() on item.ShoppingListId equals list.Id
                                     where list.PantryId == value && list.Status == ShoppingListStatus.Active && item.PantryItemId != null
                                     select new { item.PantryItemId, item.ShoppingListId }).ToListAsync(cancellationToken);
            duplicateItemIds = activeItems.GroupBy(x => x.PantryItemId!.Value)
                .Where(x => x.Select(y => y.ShoppingListId).Distinct().Count() > 1)
                .Select(x => x.Key).ToHashSet();
        }
        return lists.Select(x => new ShoppingListResponse(x.Id, x.SourcePresetId, x.SourceStocktakeId, x.Name, x.Status, x.UsesCustomOrder,
            x.StockChangedSinceGeneration, x.GeneratedAtUtc, x.CompletedAtUtc,
            items.Where(y => y.ShoppingListId == x.Id).Select(y => ToResponse(y,
                y.PantryItemId is Guid pantryItemId && duplicateItemIds.Contains(pantryItemId))).ToList(), ServiceSupport.EncodeVersion(x.Version))).ToList();
    }

    private async Task<ShoppingListResponse> MapAsync(ShoppingList list, CancellationToken cancellationToken) =>
        (await MapManyAsync([list], cancellationToken))[0];

    private static ShoppingListItemResponse ToResponse(ShoppingListItem x, bool isOnAnotherActiveList) =>
        new(x.Id, x.PantryItemId, x.PantryCategoryId, x.DestinationLocationId, x.ItemNameSnapshot, x.BrandSnapshot, x.CategoryNameSnapshot,
            x.TrackingUnitSnapshot, x.PackageSizeSnapshot, x.PackageUnitSnapshot, x.StockAtGeneration,
            x.RequiredAtGeneration, x.SuggestedPurchaseQuantity, x.ActualPurchaseQuantity, x.Outcome, x.IsManual, x.CreatePantryItemOnPurchase, isOnAnotherActiveList,
            x.SortOrder, x.InventoryAppliedAtUtc, ServiceSupport.EncodeVersion(x.Version));
}
