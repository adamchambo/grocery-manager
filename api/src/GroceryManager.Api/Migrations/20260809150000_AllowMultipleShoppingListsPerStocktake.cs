using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using GroceryManager.Api.Persistence;

#nullable disable

namespace GroceryManager.Api.Migrations;

[DbContext(typeof(GroceryManagerDbContext))]
[Migration("20260809150000_AllowMultipleShoppingListsPerStocktake")]
public partial class AllowMultipleShoppingListsPerStocktake : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_ShoppingLists_SourceStocktakeId", "ShoppingLists");
        migrationBuilder.CreateIndex("IX_ShoppingLists_SourceStocktakeId", "ShoppingLists", "SourceStocktakeId", filter: "\"SourceStocktakeId\" IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_ShoppingLists_SourceStocktakeId", "ShoppingLists");
        migrationBuilder.CreateIndex("IX_ShoppingLists_SourceStocktakeId", "ShoppingLists", "SourceStocktakeId", unique: true, filter: "\"SourceStocktakeId\" IS NOT NULL");
    }
}
