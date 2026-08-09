using GroceryManager.Api.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryManager.Api.Migrations;

[DbContext(typeof(GroceryManagerDbContext))]
[Migration("20260809123000_AddShoppingRoutine")]
public sealed class AddShoppingRoutine : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PrimaryShopName",
            table: "Pantries",
            type: "character varying(120)",
            maxLength: 120,
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "ShoppingIntervalDays",
            table: "Pantries",
            type: "numeric(18,3)",
            nullable: false,
            defaultValue: 14m);

        migrationBuilder.AddCheckConstraint(
            name: "CK_Pantries_ShoppingIntervalDays",
            table: "Pantries",
            sql: "\"ShoppingIntervalDays\" > 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(name: "CK_Pantries_ShoppingIntervalDays", table: "Pantries");
        migrationBuilder.DropColumn(name: "PrimaryShopName", table: "Pantries");
        migrationBuilder.DropColumn(name: "ShoppingIntervalDays", table: "Pantries");
    }
}
