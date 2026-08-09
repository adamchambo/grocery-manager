using GroceryManager.Api.Entities.Entitlements;
using GroceryManager.Api.Entities.Identity;
using GroceryManager.Api.Entities.InventoryHistory;
using GroceryManager.Api.Entities.Pantry;
using GroceryManager.Api.Entities.Shopping;
using GroceryManager.Api.Entities.ShoppingPresets;
using GroceryManager.Api.Entities.Stocktakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryManager.Api.Persistence;

internal static class GroceryManagerModelConfiguration
{
    private const string QuantityType = "numeric(18,3)";

    public static void Configure(ModelBuilder modelBuilder)
    {
        ConfigurePantry(modelBuilder.Entity<Pantry>());
        ConfigureCategory(modelBuilder.Entity<Category>());
        ConfigureStorageLocation(modelBuilder.Entity<StorageLocation>());
        ConfigureItemTemplate(modelBuilder.Entity<ItemTemplate>());
        ConfigurePantryItem(modelBuilder.Entity<PantryItem>());
        ConfigurePantryItemLocation(modelBuilder.Entity<PantryItemLocation>());
        ConfigureShoppingPreset(modelBuilder.Entity<ShoppingPreset>());
        ConfigurePresetCategory(modelBuilder.Entity<PresetCategory>());
        ConfigurePresetItemRule(modelBuilder.Entity<PresetItemRule>());
        ConfigureStocktake(modelBuilder.Entity<Stocktake>());
        ConfigureStocktakeEntry(modelBuilder.Entity<StocktakeEntry>());
        ConfigureShoppingList(modelBuilder.Entity<ShoppingList>());
        ConfigureShoppingListItem(modelBuilder.Entity<ShoppingListItem>());
        ConfigureInventoryAdjustment(modelBuilder.Entity<InventoryAdjustment>());
        ConfigurePlan(modelBuilder.Entity<Plan>());
        ConfigureEntitlement(modelBuilder.Entity<Entitlement>());
        ConfigurePlanEntitlement(modelBuilder.Entity<PlanEntitlement>());
    }

    private static void ConfigurePantry(EntityTypeBuilder<Pantry> builder)
    {
        builder.ToTable("Pantries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PrimaryShopName).HasMaxLength(120);
        builder.Property(x => x.ShoppingIntervalDays).HasColumnType(QuantityType).HasDefaultValue(14m);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.OwnerUserId).IsUnique();
        builder.ToTable("Pantries", table => table.HasCheckConstraint("CK_Pantries_ShoppingIntervalDays", "\"ShoppingIntervalDays\" > 0"));
        builder.HasOne<ApplicationUser>().WithOne().HasForeignKey<Pantry>(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCategory(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnType("citext").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.PantryId, x.Name }).IsUnique()
            .HasFilter("\"IsArchived\" = FALSE");
        builder.HasOne<Pantry>().WithMany().HasForeignKey(x => x.PantryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureStorageLocation(EntityTypeBuilder<StorageLocation> builder)
    {
        builder.ToTable("StorageLocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnType("citext").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.PantryId, x.Name }).IsUnique()
            .HasFilter("\"IsArchived\" = FALSE");
        builder.HasIndex(x => new { x.PantryId, x.SortOrder });
        builder.HasOne<Pantry>().WithMany().HasForeignKey(x => x.PantryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureItemTemplate(EntityTypeBuilder<ItemTemplate> builder)
    {
        builder.ToTable("ItemTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.DefaultCategoryKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.DefaultTrackingUnit).HasConversion<string>().HasMaxLength(32);
    }

    private static void ConfigurePantryItem(EntityTypeBuilder<PantryItem> builder)
    {
        builder.ToTable("PantryItems", table =>
        {
            table.HasCheckConstraint("CK_PantryItems_PackageSize", "\"PackageSize\" IS NULL OR \"PackageSize\" >= 0");
            table.HasCheckConstraint("CK_PantryItems_Consumption", "(\"ConsumptionQuantity\" IS NULL OR \"ConsumptionQuantity\" >= 0) AND (\"ConsumptionPeriodDays\" IS NULL OR \"ConsumptionPeriodDays\" > 0) AND \"BufferDays\" >= 0");
            table.HasCheckConstraint("CK_PantryItems_ConsumptionPair", "(\"ConsumptionQuantity\" IS NULL) = (\"ConsumptionPeriodDays\" IS NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnType("citext").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.Brand).HasMaxLength(120);
        builder.Property(x => x.PreferredProduct).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.TrackingUnit).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.PackageSize).HasColumnType(QuantityType);
        builder.Property(x => x.PackageUnit).HasMaxLength(32);
        builder.Property(x => x.ConsumptionQuantity).HasColumnType(QuantityType);
        builder.Property(x => x.ConsumptionPeriodDays).HasColumnType(QuantityType);
        builder.Property(x => x.BufferDays).HasColumnType(QuantityType);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.PantryId, x.CategoryId, x.Name })
            .HasFilter("\"IsArchived\" = FALSE");
        builder.HasOne<Pantry>().WithMany().HasForeignKey(x => x.PantryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ItemTemplate>().WithMany().HasForeignKey(x => x.SourceTemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StorageLocation>().WithMany().HasForeignKey(x => x.DefaultStorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePantryItemLocation(EntityTypeBuilder<PantryItemLocation> builder)
    {
        builder.ToTable("PantryItemLocations", table =>
            table.HasCheckConstraint("CK_PantryItemLocations_CurrentQuantity", "\"CurrentQuantity\" >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CurrentQuantity).HasColumnType(QuantityType);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.PantryItemId, x.StorageLocationId }).IsUnique();
        builder.HasIndex(x => new { x.StorageLocationId, x.SortOrder });
        builder.HasOne<PantryItem>().WithMany().HasForeignKey(x => x.PantryItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StorageLocation>().WithMany().HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureShoppingPreset(EntityTypeBuilder<ShoppingPreset> builder)
    {
        builder.ToTable("ShoppingPresets", table =>
            table.HasCheckConstraint("CK_ShoppingPresets_CoverageDays", "\"CoverageDays\" >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CoverageDays).HasColumnType(QuantityType);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.PantryId).HasFilter("\"IsArchived\" = FALSE");
        builder.HasOne<Pantry>().WithMany().HasForeignKey(x => x.PantryId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePresetCategory(EntityTypeBuilder<PresetCategory> builder)
    {
        builder.ToTable("PresetCategories");
        builder.HasKey(x => new { x.ShoppingPresetId, x.CategoryId });
        builder.HasOne<ShoppingPreset>().WithMany().HasForeignKey(x => x.ShoppingPresetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePresetItemRule(EntityTypeBuilder<PresetItemRule> builder)
    {
        builder.ToTable("PresetItemRules");
        builder.HasKey(x => new { x.ShoppingPresetId, x.PantryItemId });
        builder.Property(x => x.RuleType).HasConversion<string>().HasMaxLength(16);
        builder.HasOne<ShoppingPreset>().WithMany().HasForeignKey(x => x.ShoppingPresetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PantryItem>().WithMany().HasForeignKey(x => x.PantryItemId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureStocktake(EntityTypeBuilder<Stocktake> builder)
    {
        builder.ToTable("Stocktakes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.PantryId, x.StartedAtUtc }).IsDescending(false, true);
        builder.HasIndex(x => x.PantryId).IsUnique().HasFilter("\"Status\" = 'InProgress'").HasDatabaseName("IX_Stocktakes_PantryId_InProgress");
        builder.HasOne<Pantry>().WithMany().HasForeignKey(x => x.PantryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ShoppingPreset>().WithMany().HasForeignKey(x => x.ShoppingPresetId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureStocktakeEntry(EntityTypeBuilder<StocktakeEntry> builder)
    {
        builder.ToTable("StocktakeEntries", table =>
        {
            table.HasCheckConstraint("CK_StocktakeEntries_Quantities", "\"PreviousConfirmedQuantity\" >= 0 AND \"EstimatedQuantity\" >= 0 AND (\"RecordedQuantity\" IS NULL OR \"RecordedQuantity\" >= 0)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ItemNameSnapshot).HasMaxLength(160).IsRequired();
        builder.Property(x => x.LocationNameSnapshot).HasMaxLength(120).IsRequired();
        builder.Property(x => x.TrackingUnitSnapshot).HasMaxLength(32).IsRequired();
        builder.Property(x => x.PreviousConfirmedQuantity).HasColumnType(QuantityType);
        builder.Property(x => x.EstimatedQuantity).HasColumnType(QuantityType);
        builder.Property(x => x.RecordedQuantity).HasColumnType(QuantityType);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.StocktakeId, x.PantryItemLocationId }).IsUnique();
        builder.HasOne<Stocktake>().WithMany().HasForeignKey(x => x.StocktakeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PantryItemLocation>().WithMany().HasForeignKey(x => x.PantryItemLocationId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureShoppingList(EntityTypeBuilder<ShoppingList> builder)
    {
        builder.ToTable("ShoppingLists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.PantryId, x.Status });
        builder.HasIndex(x => x.SourceStocktakeId).IsUnique().HasFilter("\"SourceStocktakeId\" IS NOT NULL");
        builder.HasOne<Pantry>().WithMany().HasForeignKey(x => x.PantryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ShoppingPreset>().WithMany().HasForeignKey(x => x.SourcePresetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Stocktake>().WithOne().HasForeignKey<ShoppingList>(x => x.SourceStocktakeId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureShoppingListItem(EntityTypeBuilder<ShoppingListItem> builder)
    {
        builder.ToTable("ShoppingListItems", table =>
        {
            table.HasCheckConstraint("CK_ShoppingListItems_Quantities", "(\"PackageSizeSnapshot\" IS NULL OR \"PackageSizeSnapshot\" >= 0) AND (\"StockAtGeneration\" IS NULL OR \"StockAtGeneration\" >= 0) AND (\"RequiredAtGeneration\" IS NULL OR \"RequiredAtGeneration\" >= 0) AND (\"SuggestedPurchaseQuantity\" IS NULL OR \"SuggestedPurchaseQuantity\" >= 0) AND (\"ActualPurchaseQuantity\" IS NULL OR \"ActualPurchaseQuantity\" >= 0)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ItemNameSnapshot).HasMaxLength(160).IsRequired();
        builder.Property(x => x.BrandSnapshot).HasMaxLength(120);
        builder.Property(x => x.CategoryNameSnapshot).HasMaxLength(120);
        builder.Property(x => x.TrackingUnitSnapshot).HasMaxLength(32);
        builder.Property(x => x.PackageUnitSnapshot).HasMaxLength(32);
        builder.Property(x => x.PackageSizeSnapshot).HasColumnType(QuantityType);
        builder.Property(x => x.StockAtGeneration).HasColumnType(QuantityType);
        builder.Property(x => x.RequiredAtGeneration).HasColumnType(QuantityType);
        builder.Property(x => x.SuggestedPurchaseQuantity).HasColumnType(QuantityType);
        builder.Property(x => x.ActualPurchaseQuantity).HasColumnType(QuantityType);
        builder.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.PantryTrackingUnit).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.PantryItemId, x.Outcome });
        builder.HasOne<ShoppingList>().WithMany().HasForeignKey(x => x.ShoppingListId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PantryItem>().WithMany().HasForeignKey(x => x.PantryItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>().WithMany().HasForeignKey(x => x.PantryCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StorageLocation>().WithMany().HasForeignKey(x => x.DestinationLocationId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureInventoryAdjustment(EntityTypeBuilder<InventoryAdjustment> builder)
    {
        builder.ToTable("InventoryAdjustments", table =>
            table.HasCheckConstraint("CK_InventoryAdjustments_NotSelfReversing", "\"ReversesAdjustmentId\" IS NULL OR \"ReversesAdjustmentId\" <> \"Id\""));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AdjustmentType).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.QuantityDelta).HasColumnType(QuantityType);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL");
        builder.HasIndex(x => new { x.PantryItemLocationId, x.CreatedAtUtc }).IsDescending(false, true);
        builder.HasIndex(x => x.SourceStocktakeEntryId).IsUnique().HasFilter("\"SourceStocktakeEntryId\" IS NOT NULL");
        builder.HasIndex(x => x.SourceShoppingListItemId).IsUnique().HasFilter("\"SourceShoppingListItemId\" IS NOT NULL");
        builder.HasOne<PantryItemLocation>().WithMany().HasForeignKey(x => x.PantryItemLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StocktakeEntry>().WithOne().HasForeignKey<InventoryAdjustment>(x => x.SourceStocktakeEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ShoppingListItem>().WithOne().HasForeignKey<InventoryAdjustment>(x => x.SourceShoppingListItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InventoryAdjustment>().WithOne().HasForeignKey<InventoryAdjustment>(x => x.ReversesAdjustmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePlan(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }

    private static void ConfigureEntitlement(EntityTypeBuilder<Entitlement> builder)
    {
        builder.ToTable("Entitlements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }

    private static void ConfigurePlanEntitlement(EntityTypeBuilder<PlanEntitlement> builder)
    {
        builder.ToTable("PlanEntitlements", table =>
            table.HasCheckConstraint("CK_PlanEntitlements_LimitValue", "\"LimitValue\" IS NULL OR \"LimitValue\" >= 0"));
        builder.HasKey(x => new { x.PlanId, x.EntitlementId });
        builder.Property(x => x.LimitValue).HasColumnType(QuantityType);
        builder.HasOne<Plan>().WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Entitlement>().WithMany().HasForeignKey(x => x.EntitlementId).OnDelete(DeleteBehavior.Restrict);
    }
}
