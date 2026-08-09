using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Enums.Pantry;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Persistence;

public static class DefaultDataSeeder
{
    private static readonly (string Name, string Key)[] Categories =
    [
        ("Fruit & Vegetables", "fruit-vegetables"),
        ("Dairy & Eggs", "dairy-eggs"),
        ("Meat & Seafood", "meat-seafood"),
        ("Bakery", "bakery"),
        ("Pantry Staples", "pantry-staples"),
        ("Frozen", "frozen"),
        ("Snacks", "snacks"),
        ("Drinks", "drinks"),
        ("Household", "household"),
        ("Miscellaneous", "miscellaneous")
    ];

    private static readonly string[] Locations =
    [
        "Pantry",
        "Fridge",
        "Freezer",
        "Kitchen Cupboard",
        "Bathroom",
        "Laundry",
        "Garage / Bulk Storage"
    ];

    private static readonly (Guid Id, string Name, string CategoryKey, TrackingUnit Unit)[] ItemTemplates =
    [
        (Guid.Parse("01000000-0000-0000-0000-000000000001"), "Milk", "dairy-eggs", TrackingUnit.Volume),
        (Guid.Parse("01000000-0000-0000-0000-000000000002"), "Bread", "bakery", TrackingUnit.Package),
        (Guid.Parse("01000000-0000-0000-0000-000000000003"), "Eggs", "dairy-eggs", TrackingUnit.Item),
        (Guid.Parse("01000000-0000-0000-0000-000000000004"), "Butter", "dairy-eggs", TrackingUnit.Package),
        (Guid.Parse("01000000-0000-0000-0000-000000000005"), "Cheese", "dairy-eggs", TrackingUnit.Package),
        (Guid.Parse("01000000-0000-0000-0000-000000000006"), "Yoghurt", "dairy-eggs", TrackingUnit.Package),
        (Guid.Parse("01000000-0000-0000-0000-000000000007"), "Chicken", "meat-seafood", TrackingUnit.Weight),
        (Guid.Parse("01000000-0000-0000-0000-000000000008"), "Mince", "meat-seafood", TrackingUnit.Weight),
        (Guid.Parse("01000000-0000-0000-0000-000000000009"), "Rice", "pantry-staples", TrackingUnit.Weight),
        (Guid.Parse("01000000-0000-0000-0000-000000000010"), "Pasta", "pantry-staples", TrackingUnit.Weight),
        (Guid.Parse("01000000-0000-0000-0000-000000000011"), "Cereal", "pantry-staples", TrackingUnit.Package),
        (Guid.Parse("01000000-0000-0000-0000-000000000012"), "Flour", "pantry-staples", TrackingUnit.Weight),
        (Guid.Parse("01000000-0000-0000-0000-000000000013"), "Sugar", "pantry-staples", TrackingUnit.Weight),
        (Guid.Parse("01000000-0000-0000-0000-000000000014"), "Cooking Oil", "pantry-staples", TrackingUnit.Volume),
        (Guid.Parse("01000000-0000-0000-0000-000000000015"), "Potatoes", "fruit-vegetables", TrackingUnit.Weight),
        (Guid.Parse("01000000-0000-0000-0000-000000000016"), "Onions", "fruit-vegetables", TrackingUnit.Item),
        (Guid.Parse("01000000-0000-0000-0000-000000000017"), "Bananas", "fruit-vegetables", TrackingUnit.Item),
        (Guid.Parse("01000000-0000-0000-0000-000000000018"), "Apples", "fruit-vegetables", TrackingUnit.Item),
        (Guid.Parse("01000000-0000-0000-0000-000000000019"), "Coffee", "drinks", TrackingUnit.Package),
        (Guid.Parse("01000000-0000-0000-0000-000000000020"), "Tea", "drinks", TrackingUnit.Package),
        (Guid.Parse("01000000-0000-0000-0000-000000000021"), "Toilet Paper", "household", TrackingUnit.Package),
        (Guid.Parse("01000000-0000-0000-0000-000000000022"), "Dishwashing Liquid", "household", TrackingUnit.Volume)
    ];

    public static async Task SeedGlobalDataAsync(
        GroceryManagerDbContext db,
        CancellationToken cancellationToken = default)
    {
        var existingIds = await db.ItemTemplates.Select(x => x.Id).ToListAsync(cancellationToken);
        var existingNames = await db.ItemTemplates.Select(x => x.Name).ToListAsync(cancellationToken);

        db.ItemTemplates.AddRange(ItemTemplates
            .Select((template, index) => new ItemTemplate
            {
                Id = template.Id,
                Name = template.Name,
                DefaultCategoryKey = template.CategoryKey,
                DefaultTrackingUnit = template.Unit,
                IsActive = true,
                SortOrder = index
            })
            .Where(template => !existingIds.Contains(template.Id) &&
                !existingNames.Contains(template.Name, StringComparer.OrdinalIgnoreCase)));

        await db.SaveChangesAsync(cancellationToken);
    }

    public static void AddPantryDefaults(
        GroceryManagerDbContext db,
        Guid pantryId,
        DateTimeOffset createdAtUtc)
    {
        db.Categories.AddRange(Categories.Select((category, index) => new Category
        {
            Id = Guid.NewGuid(),
            PantryId = pantryId,
            Name = category.Name,
            SortOrder = index,
            IsDefault = true,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        }));

        db.StorageLocations.AddRange(Locations.Select((name, index) => new StorageLocation
        {
            Id = Guid.NewGuid(),
            PantryId = pantryId,
            Name = name,
            SortOrder = index,
            IsDefault = true,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        }));
    }
}
