using GroceryManager.Api.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryManager.Api.Migrations;

[DbContext(typeof(GroceryManagerDbContext))]
[Migration("20260808121500_ExpandPantryItemIconLength")]
public sealed class ExpandPantryItemIconLength : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "Icon",
            table: "PantryItems",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(16)",
            oldMaxLength: 16,
            oldNullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "Icon",
            table: "PantryItems",
            type: "character varying(16)",
            maxLength: 16,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100,
            oldNullable: true);
}
