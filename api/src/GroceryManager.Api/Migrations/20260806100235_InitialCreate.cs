using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GroceryManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entitlements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DefaultCategoryKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DefaultTrackingUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pantries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pantries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pantries_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanEntitlements",
                columns: table => new
                {
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntitlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    LimitValue = table.Column<decimal>(type: "numeric(18,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanEntitlements", x => new { x.PlanId, x.EntitlementId });
                    table.CheckConstraint("CK_PlanEntitlements_LimitValue", "\"LimitValue\" IS NULL OR \"LimitValue\" >= 0");
                    table.ForeignKey(
                        name: "FK_PlanEntitlements_Entitlements_EntitlementId",
                        column: x => x.EntitlementId,
                        principalTable: "Entitlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanEntitlements_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "citext", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Pantries_PantryId",
                        column: x => x.PantryId,
                        principalTable: "Pantries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingPresets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CoverageDays = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    IsEverythingPreset = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingPresets", x => x.Id);
                    table.CheckConstraint("CK_ShoppingPresets_CoverageDays", "\"CoverageDays\" >= 0");
                    table.ForeignKey(
                        name: "FK_ShoppingPresets_Pantries_PantryId",
                        column: x => x.PantryId,
                        principalTable: "Pantries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "citext", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageLocations_Pantries_PantryId",
                        column: x => x.PantryId,
                        principalTable: "Pantries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresetCategories",
                columns: table => new
                {
                    ShoppingPresetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresetCategories", x => new { x.ShoppingPresetId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_PresetCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PresetCategories_ShoppingPresets_ShoppingPresetId",
                        column: x => x.ShoppingPresetId,
                        principalTable: "ShoppingPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stocktakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShoppingPresetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocktakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocktakes_Pantries_PantryId",
                        column: x => x.PantryId,
                        principalTable: "Pantries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stocktakes_ShoppingPresets_ShoppingPresetId",
                        column: x => x.ShoppingPresetId,
                        principalTable: "ShoppingPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PantryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultStorageLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "citext", maxLength: 160, nullable: false),
                    Brand = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    PreferredProduct = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TrackingUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PackageSize = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    PackageUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ConsumptionQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    ConsumptionPeriodDays = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    BufferDays = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PantryItems", x => x.Id);
                    table.CheckConstraint("CK_PantryItems_Consumption", "(\"ConsumptionQuantity\" IS NULL OR \"ConsumptionQuantity\" >= 0) AND (\"ConsumptionPeriodDays\" IS NULL OR \"ConsumptionPeriodDays\" > 0) AND \"BufferDays\" >= 0");
                    table.CheckConstraint("CK_PantryItems_ConsumptionPair", "(\"ConsumptionQuantity\" IS NULL) = (\"ConsumptionPeriodDays\" IS NULL)");
                    table.CheckConstraint("CK_PantryItems_PackageSize", "\"PackageSize\" IS NULL OR \"PackageSize\" >= 0");
                    table.ForeignKey(
                        name: "FK_PantryItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PantryItems_ItemTemplates_SourceTemplateId",
                        column: x => x.SourceTemplateId,
                        principalTable: "ItemTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PantryItems_Pantries_PantryId",
                        column: x => x.PantryId,
                        principalTable: "Pantries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PantryItems_StorageLocations_DefaultStorageLocationId",
                        column: x => x.DefaultStorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePresetId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceStocktakeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StockChangedSinceGeneration = table.Column<bool>(type: "boolean", nullable: false),
                    GeneratedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingLists_Pantries_PantryId",
                        column: x => x.PantryId,
                        principalTable: "Pantries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingLists_ShoppingPresets_SourcePresetId",
                        column: x => x.SourcePresetId,
                        principalTable: "ShoppingPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingLists_Stocktakes_SourceStocktakeId",
                        column: x => x.SourceStocktakeId,
                        principalTable: "Stocktakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PantryItemLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    LastConfirmedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PantryItemLocations", x => x.Id);
                    table.CheckConstraint("CK_PantryItemLocations_CurrentQuantity", "\"CurrentQuantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_PantryItemLocations_PantryItems_PantryItemId",
                        column: x => x.PantryItemId,
                        principalTable: "PantryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PantryItemLocations_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresetItemRules",
                columns: table => new
                {
                    ShoppingPresetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresetItemRules", x => new { x.ShoppingPresetId, x.PantryItemId });
                    table.ForeignKey(
                        name: "FK_PresetItemRules_PantryItems_PantryItemId",
                        column: x => x.PantryItemId,
                        principalTable: "PantryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PresetItemRules_ShoppingPresets_ShoppingPresetId",
                        column: x => x.ShoppingPresetId,
                        principalTable: "ShoppingPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShoppingListId = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemNameSnapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    BrandSnapshot = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CategoryNameSnapshot = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    TrackingUnitSnapshot = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PackageSizeSnapshot = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    PackageUnitSnapshot = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    StockAtGeneration = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    RequiredAtGeneration = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    SuggestedPurchaseQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    ActualPurchaseQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    InventoryAppliedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingListItems", x => x.Id);
                    table.CheckConstraint("CK_ShoppingListItems_Quantities", "(\"PackageSizeSnapshot\" IS NULL OR \"PackageSizeSnapshot\" >= 0) AND (\"StockAtGeneration\" IS NULL OR \"StockAtGeneration\" >= 0) AND (\"RequiredAtGeneration\" IS NULL OR \"RequiredAtGeneration\" >= 0) AND (\"SuggestedPurchaseQuantity\" IS NULL OR \"SuggestedPurchaseQuantity\" >= 0) AND (\"ActualPurchaseQuantity\" IS NULL OR \"ActualPurchaseQuantity\" >= 0)");
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_PantryItems_PantryItemId",
                        column: x => x.PantryItemId,
                        principalTable: "PantryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_ShoppingLists_ShoppingListId",
                        column: x => x.ShoppingListId,
                        principalTable: "ShoppingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_StorageLocations_DestinationLocationId",
                        column: x => x.DestinationLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StocktakeEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StocktakeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryItemLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNameSnapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    LocationNameSnapshot = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TrackingUnitSnapshot = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LocationSortOrderSnapshot = table.Column<int>(type: "integer", nullable: false),
                    ItemSortOrderSnapshot = table.Column<int>(type: "integer", nullable: false),
                    PreviousConfirmedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    EstimatedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    RecordedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsOutlier = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StocktakeEntries", x => x.Id);
                    table.CheckConstraint("CK_StocktakeEntries_Quantities", "\"PreviousConfirmedQuantity\" >= 0 AND \"EstimatedQuantity\" >= 0 AND (\"RecordedQuantity\" IS NULL OR \"RecordedQuantity\" >= 0)");
                    table.ForeignKey(
                        name: "FK_StocktakeEntries_PantryItemLocations_PantryItemLocationId",
                        column: x => x.PantryItemLocationId,
                        principalTable: "PantryItemLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StocktakeEntries_Stocktakes_StocktakeId",
                        column: x => x.StocktakeId,
                        principalTable: "Stocktakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PantryItemLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceStocktakeEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceShoppingListItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversesAdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    QuantityDelta = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustments", x => x.Id);
                    table.CheckConstraint("CK_InventoryAdjustments_NotSelfReversing", "\"ReversesAdjustmentId\" IS NULL OR \"ReversesAdjustmentId\" <> \"Id\"");
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_InventoryAdjustments_ReversesAdjustmen~",
                        column: x => x.ReversesAdjustmentId,
                        principalTable: "InventoryAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_PantryItemLocations_PantryItemLocation~",
                        column: x => x.PantryItemLocationId,
                        principalTable: "PantryItemLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_ShoppingListItems_SourceShoppingListIt~",
                        column: x => x.SourceShoppingListItemId,
                        principalTable: "ShoppingListItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_StocktakeEntries_SourceStocktakeEntryId",
                        column: x => x.SourceStocktakeEntryId,
                        principalTable: "StocktakeEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_PantryId_Name",
                table: "Categories",
                columns: new[] { "PantryId", "Name" },
                unique: true,
                filter: "\"IsArchived\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Entitlements_Code",
                table: "Entitlements",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_CreatedByUserId",
                table: "InventoryAdjustments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_IdempotencyKey",
                table: "InventoryAdjustments",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_PantryItemLocationId_CreatedAtUtc",
                table: "InventoryAdjustments",
                columns: new[] { "PantryItemLocationId", "CreatedAtUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_ReversesAdjustmentId",
                table: "InventoryAdjustments",
                column: "ReversesAdjustmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_SourceShoppingListItemId",
                table: "InventoryAdjustments",
                column: "SourceShoppingListItemId",
                unique: true,
                filter: "\"SourceShoppingListItemId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_SourceStocktakeEntryId",
                table: "InventoryAdjustments",
                column: "SourceStocktakeEntryId",
                unique: true,
                filter: "\"SourceStocktakeEntryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pantries_OwnerUserId",
                table: "Pantries",
                column: "OwnerUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PantryItemLocations_PantryItemId_StorageLocationId",
                table: "PantryItemLocations",
                columns: new[] { "PantryItemId", "StorageLocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PantryItemLocations_StorageLocationId_SortOrder",
                table: "PantryItemLocations",
                columns: new[] { "StorageLocationId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PantryItems_CategoryId",
                table: "PantryItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PantryItems_DefaultStorageLocationId",
                table: "PantryItems",
                column: "DefaultStorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PantryItems_PantryId_CategoryId_Name",
                table: "PantryItems",
                columns: new[] { "PantryId", "CategoryId", "Name" },
                filter: "\"IsArchived\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PantryItems_SourceTemplateId",
                table: "PantryItems",
                column: "SourceTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanEntitlements_EntitlementId",
                table: "PlanEntitlements",
                column: "EntitlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Code",
                table: "Plans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PresetCategories_CategoryId",
                table: "PresetCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PresetItemRules_PantryItemId",
                table: "PresetItemRules",
                column: "PantryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_DestinationLocationId",
                table: "ShoppingListItems",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_PantryItemId_Outcome",
                table: "ShoppingListItems",
                columns: new[] { "PantryItemId", "Outcome" });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_ShoppingListId",
                table: "ShoppingListItems",
                column: "ShoppingListId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_PantryId_Status",
                table: "ShoppingLists",
                columns: new[] { "PantryId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_SourcePresetId",
                table: "ShoppingLists",
                column: "SourcePresetId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_SourceStocktakeId",
                table: "ShoppingLists",
                column: "SourceStocktakeId",
                unique: true,
                filter: "\"SourceStocktakeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingPresets_PantryId",
                table: "ShoppingPresets",
                column: "PantryId",
                filter: "\"IsArchived\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_StocktakeEntries_PantryItemLocationId",
                table: "StocktakeEntries",
                column: "PantryItemLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StocktakeEntries_StocktakeId_PantryItemLocationId",
                table: "StocktakeEntries",
                columns: new[] { "StocktakeId", "PantryItemLocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stocktakes_PantryId_StartedAtUtc",
                table: "Stocktakes",
                columns: new[] { "PantryId", "StartedAtUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Stocktakes_ShoppingPresetId",
                table: "Stocktakes",
                column: "ShoppingPresetId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_PantryId_Name",
                table: "StorageLocations",
                columns: new[] { "PantryId", "Name" },
                unique: true,
                filter: "\"IsArchived\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_PantryId_SortOrder",
                table: "StorageLocations",
                columns: new[] { "PantryId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "InventoryAdjustments");

            migrationBuilder.DropTable(
                name: "PlanEntitlements");

            migrationBuilder.DropTable(
                name: "PresetCategories");

            migrationBuilder.DropTable(
                name: "PresetItemRules");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ShoppingListItems");

            migrationBuilder.DropTable(
                name: "StocktakeEntries");

            migrationBuilder.DropTable(
                name: "Entitlements");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "ShoppingLists");

            migrationBuilder.DropTable(
                name: "PantryItemLocations");

            migrationBuilder.DropTable(
                name: "Stocktakes");

            migrationBuilder.DropTable(
                name: "PantryItems");

            migrationBuilder.DropTable(
                name: "ShoppingPresets");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "ItemTemplates");

            migrationBuilder.DropTable(
                name: "StorageLocations");

            migrationBuilder.DropTable(
                name: "Pantries");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
