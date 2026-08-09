using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Common.Exceptions;
using GroceryManager.Api.Dtos.Stocktakes;
using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Entities.Stocktakes;
using GroceryManager.Api.Enums.Stocktakes;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        if (await db.Stocktakes.AnyAsync(x => x.PantryId == pantryId && x.Status == StocktakeStatus.InProgress, cancellationToken))
            throw new ConflictException("Finish or cancel the active stocktake before starting another.");
        var itemQuery = db.PantryItems.Where(x => x.PantryId == pantryId && !x.IsArchived);

        var data = await (from item in itemQuery
                          join location in db.PantryItemLocations on item.Id equals location.PantryItemId
                          join storage in db.StorageLocations on location.StorageLocationId equals storage.Id
                          where !storage.IsArchived
                          orderby storage.SortOrder, item.Name
                          select new { Item = item, Location = location, Storage = storage }).ToListAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var stocktake = new Stocktake { Id = Guid.NewGuid(), PantryId = pantryId, Status = StocktakeStatus.InProgress, StartedAtUtc = now };
        db.Stocktakes.Add(stocktake);
        db.StocktakeEntries.AddRange(data.Select(x => new StocktakeEntry
        {
            Id = Guid.NewGuid(), StocktakeId = stocktake.Id, PantryItemLocationId = x.Location.Id,
            ItemNameSnapshot = x.Item.Name, LocationNameSnapshot = x.Storage.Name,
            TrackingUnitSnapshot = x.Item.TrackingUnit.ToString(), LocationSortOrderSnapshot = x.Storage.SortOrder,
            ItemSortOrderSnapshot = 0, PreviousConfirmedQuantity = 0,
            EstimatedQuantity = 0, Status = StocktakeEntryStatus.Pending
        }));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("Finish or cancel the active stocktake before starting another.");
        }
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
        entry.IsOutlier = false;
        await db.SaveChangesAsync(cancellationToken);
        var storageLocationId = await db.PantryItemLocations.Where(x => x.Id == entry.PantryItemLocationId).Select(x => x.StorageLocationId).SingleAsync(cancellationToken);
        return ToResponse(entry, storageLocationId);
    }

    public async Task<IReadOnlyList<StocktakeEntryResponse>> SaveLocationEntriesAsync(
        Guid stocktakeId,
        SaveStocktakeLocationEntriesRequest request,
        CancellationToken cancellationToken)
    {
        var stocktake = await FindAsync(stocktakeId, cancellationToken);
        EnsureInProgress(stocktake);
        var rows = await (from entry in db.StocktakeEntries
                          join itemLocation in db.PantryItemLocations on entry.PantryItemLocationId equals itemLocation.Id
                          where entry.StocktakeId == stocktake.Id && itemLocation.StorageLocationId == request.StorageLocationId
                          select new { Entry = entry, itemLocation.StorageLocationId }).ToListAsync(cancellationToken);
        var entries = rows.ToDictionary(x => x.Entry.Id, x => x.Entry);
        if (entries.Count != request.Entries.Count || entries.Count != request.Entries.Select(x => x.EntryId).Distinct().Count() || request.Entries.Any(x => !entries.ContainsKey(x.EntryId)))
            throw new ArgumentException("The location count must include every item exactly once.");
        if (request.Entries.Any(x => x.RecordedQuantity < 0)) throw new ArgumentOutOfRangeException(nameof(request));

        var now = DateTimeOffset.UtcNow;
        foreach (var requestEntry in request.Entries)
        {
            var entry = entries[requestEntry.EntryId];
            ServiceSupport.ApplyVersion(db, entry, requestEntry.Version);
            entry.RecordedQuantity = requestEntry.RecordedQuantity;
            entry.Status = requestEntry.RecordedQuantity == 0 ? StocktakeEntryStatus.Zero : StocktakeEntryStatus.Confirmed;
            entry.ConfirmedAtUtc = now;
            entry.IsOutlier = false;
        }
        await db.SaveChangesAsync(cancellationToken);
        return rows.Select(x => ToResponse(x.Entry, x.StorageLocationId)).ToList();
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
        var nextSortOrder = (await db.PantryItemLocations.Where(x => x.StorageLocationId == storage.Id).Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? -1) + 1;
        var location = new PantryItemLocation { Id = Guid.NewGuid(), PantryItemId = item.Id, StorageLocationId = storage.Id, SortOrder = nextSortOrder, CurrentQuantity = 0, UpdatedAtUtc = now };
        var entry = new StocktakeEntry
        {
            Id = Guid.NewGuid(), StocktakeId = stocktake.Id, PantryItemLocationId = location.Id,
            ItemNameSnapshot = item.Name, LocationNameSnapshot = storage.Name, TrackingUnitSnapshot = item.TrackingUnit.ToString(),
            LocationSortOrderSnapshot = storage.SortOrder, ItemSortOrderSnapshot = nextSortOrder, PreviousConfirmedQuantity = 0, EstimatedQuantity = 0,
            RecordedQuantity = request.RecordedQuantity, Status = request.RecordedQuantity == 0 ? StocktakeEntryStatus.Zero : StocktakeEntryStatus.Corrected,
            ConfirmedAtUtc = now
        };
        db.AddRange(item, location, entry);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(entry, storage.Id);
    }

    public async Task UpdateLocationOrderAsync(Guid stocktakeId, UpdateStocktakeLocationOrderRequest request, CancellationToken cancellationToken)
    {
        var stocktake = await FindAsync(stocktakeId, cancellationToken);
        EnsureInProgress(stocktake);
        var entries = await (from entry in db.StocktakeEntries
                             join itemLocation in db.PantryItemLocations on entry.PantryItemLocationId equals itemLocation.Id
                             where entry.StocktakeId == stocktake.Id && itemLocation.StorageLocationId == request.StorageLocationId
                             select entry).ToDictionaryAsync(x => x.PantryItemLocationId, cancellationToken);
        if (entries.Count != request.PantryItemLocationIds.Count || entries.Count != request.PantryItemLocationIds.Distinct().Count() || request.PantryItemLocationIds.Any(id => !entries.ContainsKey(id)))
            throw new ArgumentException("The stocktake order must include every item in the location exactly once.");

        for (var index = 0; index < request.PantryItemLocationIds.Count; index++)
            entries[request.PantryItemLocationIds[index]].ItemSortOrderSnapshot = index;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<StocktakeResponse> CompleteAsync(Guid stocktakeId, CompleteStocktakeRequest? request, CancellationToken cancellationToken)
    {
        var stocktake = await FindAsync(stocktakeId, cancellationToken);
        EnsureInProgress(stocktake);
        var entries = await db.StocktakeEntries.Where(x => x.StocktakeId == stocktake.Id).ToListAsync(cancellationToken);
        if (entries.Any(x => x.Status == StocktakeEntryStatus.Pending || x.RecordedQuantity is null))
            throw new ConflictException("Enter a quantity for every item before generating the shopping list.");
        var now = DateTimeOffset.UtcNow;
        stocktake.Status = StocktakeStatus.Completed; stocktake.CompletedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(stocktake, cancellationToken);
    }

    private async Task ApplyLocationItemOrdersAsync(Guid pantryId, IReadOnlyList<StocktakeLocationItemOrderRequest>? locationOrders, CancellationToken cancellationToken)
    {
        if (locationOrders is null || locationOrders.Count == 0) return;
        if (locationOrders.Select(x => x.StorageLocationId).Distinct().Count() != locationOrders.Count)
            throw new ArgumentException("Each storage location can only be ordered once.");

        foreach (var order in locationOrders)
        {
            var locationExists = await db.StorageLocations.AnyAsync(x => x.Id == order.StorageLocationId && x.PantryId == pantryId && !x.IsArchived, cancellationToken);
            if (!locationExists) throw new ArgumentException("The storage location is invalid.");

            var rows = await (from itemLocation in db.PantryItemLocations
                              join item in db.PantryItems on itemLocation.PantryItemId equals item.Id
                              where itemLocation.StorageLocationId == order.StorageLocationId && item.PantryId == pantryId && !item.IsArchived
                              select itemLocation).ToDictionaryAsync(x => x.Id, cancellationToken);
            if (rows.Count != order.PantryItemLocationIds.Count || rows.Count != order.PantryItemLocationIds.Distinct().Count() || order.PantryItemLocationIds.Any(id => !rows.ContainsKey(id)))
                throw new ArgumentException("The item order must include every active item in the storage location exactly once.");

            for (var index = 0; index < order.PantryItemLocationIds.Count; index++)
            {
                var row = rows[order.PantryItemLocationIds[index]];
                row.SortOrder = index;
                row.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }
        }
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

    private static void ValidateEntry(StocktakeEntryStatus status, decimal? quantity)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (status is StocktakeEntryStatus.Confirmed or StocktakeEntryStatus.Corrected && quantity is null)
            throw new ArgumentException("Confirmed and corrected entries require a quantity.");
        if (status is StocktakeEntryStatus.Pending) throw new ArgumentException("An entry cannot be reset to pending.");
    }

    private static void EnsureInProgress(Stocktake stocktake)
    {
        if (stocktake.Status != StocktakeStatus.InProgress) throw new ConflictException("The stocktake is no longer in progress.");
    }

    private async Task<IReadOnlyList<StocktakeResponse>> MapManyAsync(IReadOnlyList<Stocktake> stocktakes, CancellationToken cancellationToken)
    {
        var ids = stocktakes.Select(x => x.Id).ToArray();
        var entries = await db.StocktakeEntries.AsNoTracking().Where(x => ids.Contains(x.StocktakeId))
            .OrderBy(x => x.LocationSortOrderSnapshot).ThenBy(x => x.ItemSortOrderSnapshot).ToListAsync(cancellationToken);
        var storageLocationIds = await db.PantryItemLocations.AsNoTracking().Where(x => entries.Select(y => y.PantryItemLocationId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.StorageLocationId, cancellationToken);
        return stocktakes.Select(x => new StocktakeResponse(x.Id, x.ShoppingPresetId, x.Status, x.StartedAtUtc, x.CompletedAtUtc,
            entries.Where(y => y.StocktakeId == x.Id).Select(y => ToResponse(y, storageLocationIds[y.PantryItemLocationId])).ToList(), ServiceSupport.EncodeVersion(x.Version))).ToList();
    }

    private async Task<StocktakeResponse> MapAsync(Stocktake stocktake, CancellationToken cancellationToken) =>
        (await MapManyAsync([stocktake], cancellationToken))[0];

    private static StocktakeEntryResponse ToResponse(StocktakeEntry x, Guid storageLocationId) =>
        new(x.Id, x.PantryItemLocationId, storageLocationId, x.ItemNameSnapshot, x.LocationNameSnapshot, x.TrackingUnitSnapshot,
            x.PreviousConfirmedQuantity, x.EstimatedQuantity, x.RecordedQuantity, x.Status, x.IsOutlier,
            x.ConfirmedAtUtc, ServiceSupport.EncodeVersion(x.Version));
}
