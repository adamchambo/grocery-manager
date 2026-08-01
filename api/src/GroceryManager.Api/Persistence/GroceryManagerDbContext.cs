using GroceryManager.Modules.Entitlements.Entities;
using GroceryManager.Modules.Identity.Entities;
using GroceryManager.Modules.InventoryHistory.Entities;
using GroceryManager.Modules.Pantry.Entities;
using GroceryManager.Modules.Shopping.Entities;
using GroceryManager.Modules.ShoppingPresets.Entities;
using GroceryManager.Modules.Stocktakes.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Persistence;

public sealed class GroceryManagerDbContext(
    DbContextOptions<GroceryManagerDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Pantry> Pantries => Set<Pantry>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<ItemTemplate> ItemTemplates => Set<ItemTemplate>();
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();
    public DbSet<PantryItemLocation> PantryItemLocations => Set<PantryItemLocation>();
    public DbSet<ShoppingPreset> ShoppingPresets => Set<ShoppingPreset>();
    public DbSet<PresetCategory> PresetCategories => Set<PresetCategory>();
    public DbSet<PresetItemRule> PresetItemRules => Set<PresetItemRule>();
    public DbSet<Stocktake> Stocktakes => Set<Stocktake>();
    public DbSet<StocktakeEntry> StocktakeEntries => Set<StocktakeEntry>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<InventoryAdjustment> InventoryAdjustments => Set<InventoryAdjustment>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Entitlement> Entitlements => Set<Entitlement>();
    public DbSet<PlanEntitlement> PlanEntitlements => Set<PlanEntitlement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasPostgresExtension("citext");
        GroceryManagerModelConfiguration.Configure(builder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RotateConcurrencyTokens();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        RotateConcurrencyTokens();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void RotateConcurrencyTokens()
    {
        foreach (var entry in ChangeTracker.Entries()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var versionProperty = entry.Metadata.FindProperty("Version");
            if (versionProperty?.ClrType == typeof(byte[]))
            {
                entry.Property("Version").CurrentValue = Guid.NewGuid().ToByteArray();
            }
        }
    }
}
