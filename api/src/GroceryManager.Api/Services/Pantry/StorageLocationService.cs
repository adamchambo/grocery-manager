using GroceryManager.Api.Common.Exceptions;
using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.Pantry;

public sealed class StorageLocationService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : IStorageLocationService
{
    public async Task<IReadOnlyList<StorageLocationResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await db.StorageLocations.AsNoTracking().Where(x => x.PantryId == pantryId && !x.IsArchived)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => ToResponse(x)).ToListAsync(cancellationToken);
    }

    public async Task<StorageLocationResponse> CreateAsync(CreateStorageLocationRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var location = new StorageLocation
        {
            Id = Guid.NewGuid(), PantryId = pantryId, Name = request.Name.Trim(), SortOrder = request.SortOrder,
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        db.StorageLocations.Add(location);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(location);
    }

    public async Task<StorageLocationResponse> UpdateAsync(Guid locationId, UpdateStorageLocationRequest request, CancellationToken cancellationToken)
    {
        var location = await FindAsync(locationId, cancellationToken);
        ServiceSupport.ApplyVersion(db, location, request.Version);
        location.Name = request.Name.Trim();
        location.SortOrder = request.SortOrder;
        location.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(location);
    }

    public async Task ArchiveAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var location = await FindAsync(locationId, cancellationToken);
        if (await db.PantryItemLocations.AnyAsync(x => x.StorageLocationId == locationId && x.CurrentQuantity > 0, cancellationToken))
            throw new ConflictException("A location containing stock cannot be archived.");
        location.IsArchived = true;
        location.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateOrderAsync(UpdateStorageLocationOrderRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        var locations = await db.StorageLocations.Where(x => x.PantryId == pantryId && request.StorageLocationIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (locations.Count != request.StorageLocationIds.Distinct().Count())
            throw new ArgumentException("The location order contains invalid or duplicate identifiers.");
        for (var index = 0; index < request.StorageLocationIds.Count; index++)
        {
            locations[request.StorageLocationIds[index]].SortOrder = index;
            locations[request.StorageLocationIds[index]].UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateItemOrderAsync(Guid locationId, UpdateLocationItemOrderRequest request, CancellationToken cancellationToken)
    {
        _ = await FindAsync(locationId, cancellationToken);
        var rows = await db.PantryItemLocations.Where(x => x.StorageLocationId == locationId && request.PantryItemLocationIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (rows.Count != request.PantryItemLocationIds.Distinct().Count())
            throw new ArgumentException("The item order contains invalid or duplicate identifiers.");
        for (var index = 0; index < request.PantryItemLocationIds.Count; index++)
        {
            rows[request.PantryItemLocationIds[index]].SortOrder = index;
            rows[request.PantryItemLocationIds[index]].UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<StorageLocation> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await db.StorageLocations.SingleOrDefaultAsync(x => x.Id == id && x.PantryId == pantryId, cancellationToken)
            ?? throw new KeyNotFoundException("Storage location not found.");
    }

    private static StorageLocationResponse ToResponse(StorageLocation x) =>
        new(x.Id, x.Name, x.SortOrder, x.IsDefault, x.IsArchived, ServiceSupport.EncodeVersion(x.Version));
}
