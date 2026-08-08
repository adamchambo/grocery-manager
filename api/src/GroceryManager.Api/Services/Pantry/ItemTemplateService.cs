using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.Pantry;

public sealed class ItemTemplateService(GroceryManagerDbContext db) : IItemTemplateService
{
    public async Task<IReadOnlyList<ItemTemplateResponse>> ListActiveAsync(CancellationToken cancellationToken) =>
        await db.ItemTemplates.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .Select(x => new ItemTemplateResponse(x.Id, x.Name, x.DefaultCategoryKey, x.DefaultTrackingUnit, x.SortOrder))
            .ToListAsync(cancellationToken);
}
