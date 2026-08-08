using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services;

internal static class ServiceSupport
{
    public static Guid RequireUserId(ICurrentUserContext currentUser) =>
        currentUser.UserId ?? throw new UnauthorizedAccessException("An authenticated user is required.");

    public static async Task<Guid> RequirePantryIdAsync(
        GroceryManagerDbContext db,
        ICurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId(currentUser);
        return await db.Pantries
            .Where(x => x.OwnerUserId == userId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync(cancellationToken) is var pantryId && pantryId != Guid.Empty
                ? pantryId
                : throw new InvalidOperationException("The current user does not have a pantry.");
    }

    public static string EncodeVersion(byte[] version) => Convert.ToBase64String(version);

    public static void ApplyVersion(GroceryManagerDbContext db, object entity, string version)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(version);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("The version value is invalid.", nameof(version), exception);
        }

        db.Entry(entity).Property("Version").OriginalValue = bytes;
    }

    public static (int Page, int PageSize) NormalizePage(int page, int pageSize) =>
        (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
