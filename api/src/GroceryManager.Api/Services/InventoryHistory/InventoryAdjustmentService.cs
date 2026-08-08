using GroceryManager.Api.Common.Dtos;
using GroceryManager.Api.Dtos.InventoryHistory;
using GroceryManager.Api.Entities.InventoryHistory;
using GroceryManager.Api.Enums.InventoryHistory;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.InventoryHistory;

public sealed class InventoryAdjustmentService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : IInventoryAdjustmentService
{
    public async Task<PagedResponse<InventoryAdjustmentResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        (page, pageSize) = ServiceSupport.NormalizePage(page, pageSize);
        var query = OwnedQuery(pantryId).AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => ToResponse(x)).ToListAsync(cancellationToken);
        return new(rows, page, pageSize, total);
    }

    public async Task<InventoryAdjustmentResponse> GetAsync(Guid adjustmentId, CancellationToken cancellationToken) =>
        ToResponse(await FindAsync(adjustmentId, cancellationToken));

    public async Task<InventoryAdjustmentResponse> CreateAsync(CreateInventoryAdjustmentRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        var location = await OwnedLocations(pantryId).SingleOrDefaultAsync(x => x.Id == request.PantryItemLocationId, cancellationToken)
            ?? throw new KeyNotFoundException("Pantry item location not found.");
        var existing = await db.InventoryAdjustments.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey, cancellationToken);
        if (existing is not null) return ToResponse(existing);
        if (location.CurrentQuantity + request.QuantityDelta < 0)
            throw new InvalidOperationException("The adjustment would make inventory negative.");

        var adjustment = Create(location.Id, request.QuantityDelta, request.Notes, request.IdempotencyKey,
            InventoryAdjustmentType.Correction, null);
        location.CurrentQuantity += request.QuantityDelta;
        location.UpdatedAtUtc = DateTimeOffset.UtcNow;
        db.InventoryAdjustments.Add(adjustment);
        await MarkOtherListsChangedAsync(location.PantryItemId, null, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(adjustment);
    }

    public async Task<InventoryAdjustmentResponse> ReverseAsync(Guid adjustmentId, ReverseInventoryAdjustmentRequest request, CancellationToken cancellationToken)
    {
        var original = await FindAsync(adjustmentId, cancellationToken);
        var existing = await db.InventoryAdjustments.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey, cancellationToken);
        if (existing is not null) return ToResponse(existing);
        if (await db.InventoryAdjustments.AnyAsync(x => x.ReversesAdjustmentId == original.Id, cancellationToken))
            throw new InvalidOperationException("This adjustment has already been reversed.");
        var location = await db.PantryItemLocations.SingleAsync(x => x.Id == original.PantryItemLocationId, cancellationToken);
        if (location.CurrentQuantity - original.QuantityDelta < 0)
            throw new InvalidOperationException("The reversal would make inventory negative.");

        var reversal = Create(location.Id, -original.QuantityDelta, request.Notes, request.IdempotencyKey,
            InventoryAdjustmentType.Reversal, original.Id);
        location.CurrentQuantity += reversal.QuantityDelta;
        location.UpdatedAtUtc = DateTimeOffset.UtcNow;
        db.InventoryAdjustments.Add(reversal);
        await MarkOtherListsChangedAsync(location.PantryItemId, original.SourceShoppingListItemId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(reversal);
    }

    private IQueryable<InventoryAdjustment> OwnedQuery(Guid pantryId) =>
        from adjustment in db.InventoryAdjustments
        join location in db.PantryItemLocations on adjustment.PantryItemLocationId equals location.Id
        join item in db.PantryItems on location.PantryItemId equals item.Id
        where item.PantryId == pantryId
        select adjustment;

    private IQueryable<Entities.Pantry.PantryItemLocation> OwnedLocations(Guid pantryId) =>
        from location in db.PantryItemLocations
        join item in db.PantryItems on location.PantryItemId equals item.Id
        where item.PantryId == pantryId
        select location;

    private async Task<InventoryAdjustment> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await OwnedQuery(pantryId).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Inventory adjustment not found.");
    }

    private InventoryAdjustment Create(Guid locationId, decimal delta, string? notes, string key,
        InventoryAdjustmentType type, Guid? reversesId) => new()
    {
        Id = Guid.NewGuid(), PantryItemLocationId = locationId, CreatedByUserId = ServiceSupport.RequireUserId(currentUser),
        AdjustmentType = type, QuantityDelta = delta, Notes = notes?.Trim(), IdempotencyKey = key,
        ReversesAdjustmentId = reversesId, CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private async Task MarkOtherListsChangedAsync(Guid pantryItemId, Guid? sourceListItemId, CancellationToken cancellationToken)
    {
        var affected = await (from list in db.ShoppingLists
                              join item in db.ShoppingListItems on list.Id equals item.ShoppingListId
                              where list.Status == Enums.Shopping.ShoppingListStatus.Active && item.PantryItemId == pantryItemId && item.Id != sourceListItemId
                              select list).Distinct().ToListAsync(cancellationToken);
        foreach (var list in affected) list.StockChangedSinceGeneration = true;
    }

    private static InventoryAdjustmentResponse ToResponse(InventoryAdjustment x) =>
        new(x.Id, x.PantryItemLocationId, x.SourceStocktakeEntryId, x.SourceShoppingListItemId,
            x.ReversesAdjustmentId, x.AdjustmentType, x.QuantityDelta, x.Notes, x.CreatedAtUtc);
}
