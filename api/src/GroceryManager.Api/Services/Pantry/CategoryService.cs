using GroceryManager.Api.Dtos.Pantry;
using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.Pantry;

public sealed class CategoryService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await db.Categories.AsNoTracking().Where(x => x.PantryId == pantryId && !x.IsArchived)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => ToResponse(x)).ToListAsync(cancellationToken);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var category = new Category
        {
            Id = Guid.NewGuid(), PantryId = pantryId, Name = request.Name.Trim(), SortOrder = request.SortOrder,
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await FindAsync(categoryId, cancellationToken);
        ServiceSupport.ApplyVersion(db, category, request.Version);
        category.Name = request.Name.Trim();
        category.SortOrder = request.SortOrder;
        category.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(category);
    }

    public async Task ArchiveAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await FindAsync(categoryId, cancellationToken);
        if (await db.PantryItems.AnyAsync(x => x.CategoryId == categoryId && !x.IsArchived, cancellationToken))
            throw new InvalidOperationException("A category containing active pantry items cannot be archived.");
        category.IsArchived = true;
        category.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateOrderAsync(UpdateCategoryOrderRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        var categories = await db.Categories.Where(x => x.PantryId == pantryId && request.CategoryIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (categories.Count != request.CategoryIds.Distinct().Count())
            throw new ArgumentException("The category order contains invalid or duplicate category identifiers.");
        for (var index = 0; index < request.CategoryIds.Count; index++)
        {
            categories[request.CategoryIds[index]].SortOrder = index;
            categories[request.CategoryIds[index]].UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Category> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await db.Categories.SingleOrDefaultAsync(x => x.Id == id && x.PantryId == pantryId, cancellationToken)
            ?? throw new KeyNotFoundException("Category not found.");
    }

    private static CategoryResponse ToResponse(Category x) =>
        new(x.Id, x.Name, x.SortOrder, x.IsDefault, x.IsArchived, ServiceSupport.EncodeVersion(x.Version));
}
