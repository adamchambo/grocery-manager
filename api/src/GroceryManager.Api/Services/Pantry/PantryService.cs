using GroceryManager.Api.Common.Exceptions;
using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.Pantry;

public sealed class PantryService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : IPantryService
{
    public async Task<PantryResponse> CreateAsync(CreatePantryRequest request, CancellationToken cancellationToken)
    {
        var userId = ServiceSupport.RequireUserId(currentUser);
        if (await db.Pantries.AnyAsync(x => x.OwnerUserId == userId, cancellationToken))
            throw new ConflictException("The current user already has a pantry.");

        var now = DateTimeOffset.UtcNow;
        var pantry = new Entities.Pantry.Pantry
        {
            Id = Guid.NewGuid(), OwnerUserId = userId, Name = request.Name.Trim(),
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        db.Pantries.Add(pantry);
        DefaultDataSeeder.AddPantryDefaults(db, pantry.Id, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(pantry);
    }

    public async Task<PantryResponse> GetCurrentAsync(CancellationToken cancellationToken) =>
        ToResponse(await GetCurrentEntityAsync(cancellationToken));

    public async Task<PantryResponse> UpdateCurrentAsync(UpdatePantryRequest request, CancellationToken cancellationToken)
    {
        var pantry = await GetCurrentEntityAsync(cancellationToken);
        ServiceSupport.ApplyVersion(db, pantry, request.Version);
        pantry.Name = request.Name.Trim();
        pantry.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(pantry);
    }

    private async Task<Entities.Pantry.Pantry> GetCurrentEntityAsync(CancellationToken cancellationToken)
    {
        var userId = ServiceSupport.RequireUserId(currentUser);
        return await db.Pantries.SingleOrDefaultAsync(x => x.OwnerUserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Pantry not found.");
    }

    private static PantryResponse ToResponse(Entities.Pantry.Pantry pantry) =>
        new(pantry.Id, pantry.Name, pantry.CreatedAtUtc, pantry.UpdatedAtUtc,
            ServiceSupport.EncodeVersion(pantry.Version));
}
