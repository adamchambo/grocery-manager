using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Common.Exceptions;
using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.Pantry;

public sealed class PantryItemService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : IPantryItemService
{
    public async Task<PagedResponse<PantryItemResponse>> ListAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        (page, pageSize) = ServiceSupport.NormalizePage(page, pageSize);
        var query = db.PantryItems.AsNoTracking().Where(x => x.PantryId == pantryId && !x.IsArchived);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search.Trim()}%"));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new(await MapManyAsync(items, cancellationToken), page, pageSize, total);
    }

    public async Task<PantryItemResponse> GetAsync(Guid itemId, CancellationToken cancellationToken) =>
        await MapAsync(await FindAsync(itemId, cancellationToken), cancellationToken);

    public async Task<PantryItemResponse> CreateAsync(CreatePantryItemRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        await ValidateReferencesAsync(pantryId, request.CategoryId, request.DefaultStorageLocationId,
            request.SourceTemplateId, request.Locations.Select(x => x.StorageLocationId), cancellationToken);
        ValidateQuantities(request.PackageSize, request.ConsumptionQuantity, request.ConsumptionPeriodDays,
            request.BufferDays, request.Locations.Select(x => x.CurrentQuantity));
        if (request.Locations.Select(x => x.StorageLocationId).Distinct().Count() != request.Locations.Count)
            throw new ArgumentException("A storage location may only appear once.");

        var now = DateTimeOffset.UtcNow;
        var item = new PantryItem
        {
            Id = Guid.NewGuid(), PantryId = pantryId, CategoryId = request.CategoryId,
            SourceTemplateId = request.SourceTemplateId, DefaultStorageLocationId = request.DefaultStorageLocationId,
            Name = request.Name.Trim(), Brand = request.Brand?.Trim(), PreferredProduct = request.PreferredProduct?.Trim(),
            Notes = request.Notes?.Trim(), TrackingUnit = request.TrackingUnit, PackageSize = request.PackageSize,
            PackageUnit = request.PackageUnit?.Trim(), ConsumptionQuantity = request.ConsumptionQuantity,
            ConsumptionPeriodDays = request.ConsumptionPeriodDays, BufferDays = request.BufferDays,
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        db.PantryItems.Add(item);
        db.PantryItemLocations.AddRange(request.Locations.Select(x => new PantryItemLocation
        {
            Id = Guid.NewGuid(), PantryItemId = item.Id, StorageLocationId = x.StorageLocationId,
            CurrentQuantity = x.CurrentQuantity, SortOrder = x.SortOrder, UpdatedAtUtc = now,
            LastConfirmedAtUtc = x.CurrentQuantity > 0 ? now : null
        }));
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(item, cancellationToken);
    }

    public async Task<PantryItemResponse> UpdateAsync(Guid itemId, UpdatePantryItemRequest request, CancellationToken cancellationToken)
    {
        var item = await FindAsync(itemId, cancellationToken);
        await ValidateReferencesAsync(item.PantryId, request.CategoryId, request.DefaultStorageLocationId, null, [], cancellationToken);
        ValidateQuantities(request.PackageSize, request.ConsumptionQuantity, request.ConsumptionPeriodDays, request.BufferDays, []);
        ServiceSupport.ApplyVersion(db, item, request.Version);
        item.CategoryId = request.CategoryId;
        item.DefaultStorageLocationId = request.DefaultStorageLocationId;
        item.Name = request.Name.Trim(); item.Brand = request.Brand?.Trim();
        item.PreferredProduct = request.PreferredProduct?.Trim(); item.Notes = request.Notes?.Trim();
        item.TrackingUnit = request.TrackingUnit; item.PackageSize = request.PackageSize;
        item.PackageUnit = request.PackageUnit?.Trim(); item.ConsumptionQuantity = request.ConsumptionQuantity;
        item.ConsumptionPeriodDays = request.ConsumptionPeriodDays; item.BufferDays = request.BufferDays;
        item.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(item, cancellationToken);
    }

    public async Task ArchiveAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await FindAsync(itemId, cancellationToken);
        item.IsArchived = true;
        item.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PantryItemResponse> UpdateLocationsAsync(
        Guid itemId, UpdatePantryItemLocationsRequest request, CancellationToken cancellationToken)
    {
        var item = await FindAsync(itemId, cancellationToken);
        ServiceSupport.ApplyVersion(db, item, request.ItemVersion);
        if (request.Locations.Select(x => x.StorageLocationId).Distinct().Count() != request.Locations.Count)
            throw new ArgumentException("A storage location may only appear once.");
        await ValidateReferencesAsync(item.PantryId, item.CategoryId, item.DefaultStorageLocationId, null,
            request.Locations.Select(x => x.StorageLocationId), cancellationToken);
        ValidateQuantities(item.PackageSize, item.ConsumptionQuantity, item.ConsumptionPeriodDays,
            item.BufferDays, request.Locations.Select(x => x.CurrentQuantity));

        var existing = await db.PantryItemLocations.Where(x => x.PantryItemId == item.Id)
            .ToDictionaryAsync(x => x.StorageLocationId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        foreach (var requested in request.Locations)
        {
            if (!existing.Remove(requested.StorageLocationId, out var row))
            {
                row = new PantryItemLocation { Id = Guid.NewGuid(), PantryItemId = item.Id, StorageLocationId = requested.StorageLocationId };
                db.PantryItemLocations.Add(row);
            }
            row.CurrentQuantity = requested.CurrentQuantity;
            row.SortOrder = requested.SortOrder;
            row.UpdatedAtUtc = now;
        }
        if (existing.Values.Any(x => x.CurrentQuantity != 0))
            throw new ConflictException("A location containing stock cannot be removed from an item.");
        db.PantryItemLocations.RemoveRange(existing.Values);
        item.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(item, cancellationToken);
    }

    private async Task<PantryItem> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await db.PantryItems.SingleOrDefaultAsync(x => x.Id == id && x.PantryId == pantryId, cancellationToken)
            ?? throw new KeyNotFoundException("Pantry item not found.");
    }

    private async Task ValidateReferencesAsync(Guid pantryId, Guid categoryId, Guid? defaultLocationId,
        Guid? templateId, IEnumerable<Guid> locationIds, CancellationToken cancellationToken)
    {
        if (!await db.Categories.AnyAsync(x => x.Id == categoryId && x.PantryId == pantryId && !x.IsArchived, cancellationToken))
            throw new ArgumentException("The category is invalid.");
        var ids = locationIds.Append(defaultLocationId ?? Guid.Empty).Where(x => x != Guid.Empty).Distinct().ToArray();
        if (ids.Length != await db.StorageLocations.CountAsync(x => ids.Contains(x.Id) && x.PantryId == pantryId && !x.IsArchived, cancellationToken))
            throw new ArgumentException("One or more storage locations are invalid.");
        if (templateId is not null && !await db.ItemTemplates.AnyAsync(x => x.Id == templateId && x.IsActive, cancellationToken))
            throw new ArgumentException("The item template is invalid.");
    }

    private static void ValidateQuantities(decimal? packageSize, decimal? consumptionQuantity,
        decimal? consumptionPeriodDays, decimal bufferDays, IEnumerable<decimal> quantities)
    {
        if (packageSize < 0 || consumptionQuantity < 0 || consumptionPeriodDays <= 0 || bufferDays < 0 || quantities.Any(x => x < 0))
            throw new ArgumentOutOfRangeException(nameof(quantities), "Quantities and buffer days cannot be negative.");
        if (consumptionQuantity.HasValue != consumptionPeriodDays.HasValue)
            throw new ArgumentException("Consumption quantity and period days must be supplied together.");
    }

    private async Task<IReadOnlyList<PantryItemResponse>> MapManyAsync(List<PantryItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0) return [];
        var itemIds = items.Select(x => x.Id).ToArray();
        var categoryIds = items.Select(x => x.CategoryId).Distinct().ToArray();
        var categories = await db.Categories.AsNoTracking().Where(x => categoryIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        var locations = await db.PantryItemLocations.AsNoTracking().Where(x => itemIds.Contains(x.PantryItemId)).ToListAsync(cancellationToken);
        var storageIds = locations.Select(x => x.StorageLocationId).Distinct().ToArray();
        var names = await db.StorageLocations.AsNoTracking().Where(x => storageIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        return items.Select(x => ToResponse(x, categories[x.CategoryId], locations.Where(y => y.PantryItemId == x.Id), names)).ToList();
    }

    private async Task<PantryItemResponse> MapAsync(PantryItem item, CancellationToken cancellationToken) =>
        (await MapManyAsync([item], cancellationToken))[0];

    private static PantryItemResponse ToResponse(PantryItem x, string categoryName,
        IEnumerable<PantryItemLocation> locations, Dictionary<Guid, string> locationNames) =>
        new(x.Id, x.CategoryId, categoryName, x.SourceTemplateId, x.DefaultStorageLocationId, x.Name, x.Brand,
            x.PreferredProduct, x.Notes, x.TrackingUnit, x.PackageSize, x.PackageUnit, x.ConsumptionQuantity,
            x.ConsumptionPeriodDays, x.BufferDays, x.IsArchived,
            locations.OrderBy(y => y.SortOrder).Select(y => new PantryItemLocationResponse(y.Id, y.StorageLocationId,
                locationNames[y.StorageLocationId], y.CurrentQuantity, y.SortOrder, y.LastConfirmedAtUtc,
                ServiceSupport.EncodeVersion(y.Version))).ToList(),
            x.CreatedAtUtc, x.UpdatedAtUtc, ServiceSupport.EncodeVersion(x.Version));
}
