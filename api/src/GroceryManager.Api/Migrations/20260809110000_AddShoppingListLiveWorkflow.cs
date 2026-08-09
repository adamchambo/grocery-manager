using GroceryManager.Api.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryManager.Api.Migrations;

[DbContext(typeof(GroceryManagerDbContext))]
[Migration("20260809110000_AddShoppingListLiveWorkflow")]
public sealed class AddShoppingListLiveWorkflow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(name: "UsesCustomOrder", table: "ShoppingLists", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(name: "CreatePantryItemOnPurchase", table: "ShoppingListItems", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<Guid>(name: "PantryCategoryId", table: "ShoppingListItems", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<string>(name: "PantryTrackingUnit", table: "ShoppingListItems", type: "character varying(32)", maxLength: 32, nullable: true);
        migrationBuilder.CreateIndex(name: "IX_ShoppingListItems_PantryCategoryId", table: "ShoppingListItems", column: "PantryCategoryId");
        migrationBuilder.AddForeignKey(
            name: "FK_ShoppingListItems_Categories_PantryCategoryId",
            table: "ShoppingListItems",
            column: "PantryCategoryId",
            principalTable: "Categories",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_ShoppingListItems_Categories_PantryCategoryId", table: "ShoppingListItems");
        migrationBuilder.DropIndex(name: "IX_ShoppingListItems_PantryCategoryId", table: "ShoppingListItems");
        migrationBuilder.DropColumn(name: "UsesCustomOrder", table: "ShoppingLists");
        migrationBuilder.DropColumn(name: "CreatePantryItemOnPurchase", table: "ShoppingListItems");
        migrationBuilder.DropColumn(name: "PantryCategoryId", table: "ShoppingListItems");
        migrationBuilder.DropColumn(name: "PantryTrackingUnit", table: "ShoppingListItems");
    }
}
