using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.Stocktakes;
using GroceryManager.Api.Entities.InventoryHistory;
using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Entities.Stocktakes;
using GroceryManager.Api.Enums.InventoryHistory;
using GroceryManager.Api.Enums.ShoppingPresets;
using GroceryManager.Api.Enums.Stocktakes;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.Stocktakes;

public sealed class StocktakeService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : IStocktakeService
{
    public async Task<PagedResponse<StocktakeResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        (page, pageSize) = ServiceSupport.NormalizePage(page, pageSize);
        var query = db.Stocktakes.AsNoTracking().Where(x => x.PantryId == pantryId);
        var total = await query.CountAsync(cancellationToken);
        var stocktakes = await query.OrderByDescending(x => x.StartedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new(await MapManyAsync(stocktakes, cancellationToken), page, pageSize, total);
    }

    public async Task<StocktakeResponse> GetAsync(Guid stocktakeId, CancellationToken cancellationToken) =>
        await MapAsync(await FindAsync(stocktakeId, cancellationToken), cancellationToken);

    public async Task<StocktakeResponse> StartAsync(StartStocktakeRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        if (request.ShoppingPresetId is not null && !await db.ShoppingPresets.AnyAsync(x => x.Id == request.ShoppingPresetId && x.PantryId == pantryId && !x.IsArchived, cancellationToken))
            throw new ArgumentException("The shopping preset is invalid.");

        var itemQuery = db.PantryItems.Where(x => x.PantryId == pantryId && !x.IsArchived);
        if (request.ShoppingPresetId is Guid presetId)
        {
            var preset = await db.ShoppingPresets.SingleAsync(x => x.Id == presetId, cancellationToken);
            if (!preset.IsEverythingPreset)
            {
                var categoryIds = db.PresetCategories.Where(x => x.ShoppingPresetId == presetId).Select(x => x.CategoryId);
                var includedIds = db.PresetItemRules.Where(x => x.ShoppingPresetId == presetId && x.RuleType == PresetItemRuleType.Include).Select(x => x.PantryItemId);
                var excludedIds = db.PresetItemRules.Where(x => x.ShoppingPresetId == presetId && x.RuleType == PresetItemRuleType.Exclude).Select(x => x.PantryItemId);
                itemQuery = itemQuery.Where(x => (categoryIds.Contains(x.CategoryId) || includedIds.Contains(x.Id)) && !excludedIds.Contains(x.Id));
            }
        }

        var data = await (from item in itemQuery
                          join location in db.PantryItemLocations on item.Id equals location.PantryItemId
                          join storage in db.StorageLocations on location.StorageLocationId equals storage.Id
                          where !storage.IsArchived
                          orderby storage.SortOrder, location.SortOrder
                          select new { Item = item, Location = location, Storage = storage }).ToListAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var stocktake = new Stocktake { Id = Guid.NewGuid(), PantryId = pantryId, ShoppingPresetId = request.ShoppingPresetId, Status = StocktakeStatus.InProgress, StartedAtUtc = now };
        db.Stocktakes.Add(stocktake);
        db.StocktakeEntries.AddRange(data.Select(x => new StocktakeEntry
        {
            Id = Guid.NewGuid(), StocktakeId = stocktake.Id, PantryItemLocationId = x.Location.Id,
            ItemNameSnapshot = x.Item.Name, LocationNameSnapshot = x.Storage.Name,
            TrackingUnitSnapshot = x.Item.TrackingUnit.ToString(), LocationSortOrderSnapshot = x.Storage.SortOrder,
            ItemSortOrderSnapshot = x.Location.SortOrder, PreviousConfirmedQuantity = x.Location.CurrentQuantity,
            EstimatedQuantity = Estimate(x.Item, x.Location, now), Status = StocktakeEntryStatus.Pending
        }));
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(stocktake, cancellationToken);
    }

    public async Task<StocktakeEntryResponse> UpdateEntryAsync(Guid stocktakeId, Guid entryId, UpdateStocktakeEntryRequest request, CancellationToken cancellationToken)
    {
        var stocktake = await FindAsync(stocktakeId, cancellationToken);
        EnsureInProgress(stocktake);
        var entry = await db.StocktakeEntries.SingleOrDefaultAsync(x => x.Id == entryId && x.StocktakeId == stocktake.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Stocktake entry not found.");
        ValidateEntry(request.Status, request.RecordedQuantity);
        ServiceSupport.ApplyVersion(db, entry, request.Version);
        entry.Status = request.Status;
        entry.RecordedQuantity = request.Status == StocktakeEntryStatus.Zero ? 0 : request.RecordedQuantity;
        entry.ConfirmedAtUtc = request.Status is StocktakeEntryStatus.Confirmed or StocktakeEntryStatus.Corrected or StocktakeEntryStatus.Zero ? DateTimeOffset.UtcNow : null;
        entry.IsOutlier = entry.RecordedQuantity is decimal value && Math.Abs(value - entry.EstimatedQuantity) > Math.Max(1m, entry.EstimatedQuantity * 0.5m);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(entry);
    }

    public async Task<StocktakeEntryResponse> AddDiscoveredItemAsync(Guid stocktakeId, AddDiscoveredStocktakeItemRequest request, CancellationToken cancellationToken)
    {
        var stocktake = await FindAsync(stocktakeId, cancellationToken);
        EnsureInProgress(stocktake);
        if (request.RecordedQuantity < 0) throw new ArgumentOutOfRangeException(nameof(request));
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == request.CategoryId && x.PantryId == stocktake.PantryId && !x.IsArchived, cancellationToken)
            ?? throw new ArgumentException("The category is invalid.");
        var storage = await db.StorageLocations.SingleOrDefaultAsync(x => x.Id == request.StorageLocationId && x.PantryId == stocktake.PantryId && !x.IsArchived, cancellationToken)
            ?? throw new ArgumentException("The storage location is invalid.");
        var now = DateTimeOffset.UtcNow;
        var item = new PantryItem { Id = Guid.NewGuid(), PantryId = stocktake.PantryId, CategoryId = category.Id, DefaultStorageLocationId = storage.Id, Name = request.Name.Trim(), TrackingUnit = request.TrackingUnit, BufferDays = 0, CreatedAtUtc = now, UpdatedAtUtc = now };
        var location = new PantryItemLocation { Id = Guid.NewGuid(), PantryItemId = item.Id, StorageLocationId = storage.Id, CurrentQuantity = 0, UpdatedAtUtc = now };
        var entry = new StocktakeEntry
        {
            Id = Guid.NewGuid(), StocktakeId = stocktake.Id, PantryItemLocationId = location.Id,
            ItemNameSnapshot = item.Name, LocationNameSnapshot = storage.Name, TrackingUnitSnapshot = item.TrackingUnit.ToString(),
            LocationSortOrderSnapshot = storage.SortOrder, PreviousConfirmedQuantity = 0, EstimatedQuantity = 0,
            RecordedQuantity = request.RecordedQuantity, Status = request.RecordedQuantity == 0 ? StocktakeEntryStatus.Zero : StocktakeEntryStatus.Corrected,
            ConfirmedAtUtc = now
        };
        db.AddRange(item, location, entry);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(entry);
    }

    public async Task<StocktakeResponse> CompleteAsync(Guid stocktakeId, CancellationToken cancellationToken)
    {
        var stocktake = await FindAsync(stocktakeId, cancellationToken);
        EnsureInProgress(stocktake);
        var entries = await db.StocktakeEntries.Where(x => x.StocktakeId == stocktake.Id).ToListAsync(cancellationToken);
        if (entries.Any(x => x.Status == StocktakeEntryStatus.Pending))
            throw new InvalidOperationException("Every stocktake entry must be confirmed, corrected, zero, or skipped.");
        var locationIds = entries.Select(x => x.PantryItemLocationId).ToArray();
        var locations = await db.PantryItemLocations.Where(x => locationIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in entries.Where(x => x.Status != StocktakeEntryStatus.Skipped))
        {
            var location = locations[entry.PantryItemLocationId];
            var quantity = entry.RecordedQuantity ?? throw new InvalidOperationException("A confirmed entry requires a quantity.");
            var delta = quantity - location.CurrentQuantity;
            location.CurrentQuantity = quantity; location.LastConfirmedAtUtc = now; location.UpdatedAtUtc = now;
            db.InventoryAdjustments.Add(new InventoryAdjustment
            {
                Id = Guid.NewGuid(), PantryItemLocationId = location.Id, SourceStocktakeEntryId = entry.Id,
                CreatedByUserId = ServiceSupport.RequireUserId(currentUser), AdjustmentType = InventoryAdjustmentType.StocktakeConfirmation,
                QuantityDelta = delta, IdempotencyKey = $"stocktake:{entry.Id}", CreatedAtUtc = now
            });
        }
        stocktake.Status = StocktakeStatus.Completed; stocktake.CompletedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(stocktake, cancellationToken);
    }

    public async Task<StocktakeResponse> CancelAsync(Guid stocktakeId, CancellationToken cancellationToken)
    {
        var stocktake = await FindAsync(stocktakeId, cancellationToken);
        EnsureInProgress(stocktake);
        stocktake.Status = StocktakeStatus.Cancelled;
        stocktake.CompletedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(stocktake, cancellationToken);
    }

    private async Task<Stocktake> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await db.Stocktakes.SingleOrDefaultAsync(x => x.Id == id && x.PantryId == pantryId, cancellationToken)
            ?? throw new KeyNotFoundException("Stocktake not found.");
    }

    private static decimal Estimate(PantryItem item, PantryItemLocation location, DateTimeOffset now)
    {
        if (item.ConsumptionQuantity is not decimal quantity || item.ConsumptionPeriodDays is not decimal days || location.LastConfirmedAtUtc is null)
            return location.CurrentQuantity;
        var elapsedDays = Math.Max(0m, (decimal)(now - location.LastConfirmedAtUtc.Value).TotalDays);
        return Math.Max(0m, location.CurrentQuantity - quantity / days * elapsedDays);
    }

    private static void ValidateEntry(StocktakeEntryStatus status, decimal? quantity)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (status is StocktakeEntryStatus.Confirmed or StocktakeEntryStatus.Corrected && quantity is null)
            throw new ArgumentException("Confirmed and corrected entries require a quantity.");
        if (status is StocktakeEntryStatus.Pending) throw new ArgumentException("An entry cannot be reset to pending.");
    }

    private static void EnsureInProgress(Stocktake stocktake)
    {
        if (stocktake.Status != StocktakeStatus.InProgress) throw new InvalidOperationException("The stocktake is no longer in progress.");
    }

    private async Task<IReadOnlyList<StocktakeResponse>> MapManyAsync(IReadOnlyList<Stocktake> stocktakes, CancellationToken cancellationToken)
    {
        var ids = stocktakes.Select(x => x.Id).ToArray();
        var entries = await db.StocktakeEntries.AsNoTracking().Where(x => ids.Contains(x.StocktakeId))
            .OrderBy(x => x.LocationSortOrderSnapshot).ThenBy(x => x.ItemSortOrderSnapshot).ToListAsync(cancellationToken);
        return stocktakes.Select(x => new StocktakeResponse(x.Id, x.ShoppingPresetId, x.Status, x.StartedAtUtc, x.CompletedAtUtc,
            entries.Where(y => y.StocktakeId == x.Id).Select(ToResponse).ToList(), ServiceSupport.EncodeVersion(x.Version))).ToList();
    }

    private async Task<StocktakeResponse> MapAsync(Stocktake stocktake, CancellationToken cancellationToken) =>
        (await MapManyAsync([stocktake], cancellationToken))[0];

    private static StocktakeEntryResponse ToResponse(StocktakeEntry x) =>
        new(x.Id, x.PantryItemLocationId, x.ItemNameSnapshot, x.LocationNameSnapshot, x.TrackingUnitSnapshot,
            x.PreviousConfirmedQuantity, x.EstimatedQuantity, x.RecordedQuantity, x.Status, x.IsOutlier,
            x.ConfirmedAtUtc, ServiceSupport.EncodeVersion(x.Version));
}
