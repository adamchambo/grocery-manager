using GroceryManager.Api.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryManager.Api.Migrations;

[DbContext(typeof(GroceryManagerDbContext))]
[Migration("20260809090000_EnsureSingleActiveStocktake")]
public sealed class EnsureSingleActiveStocktake : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.CreateIndex(
            name: "IX_Stocktakes_PantryId_InProgress",
            table: "Stocktakes",
            column: "PantryId",
            unique: true,
            filter: "\"Status\" = 'InProgress'");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropIndex(
            name: "IX_Stocktakes_PantryId_InProgress",
            table: "Stocktakes");
}
