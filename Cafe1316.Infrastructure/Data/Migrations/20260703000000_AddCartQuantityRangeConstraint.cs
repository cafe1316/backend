using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cafe1316.Infrastructure.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260703000000_AddCartQuantityRangeConstraint")]
public partial class AddCartQuantityRangeConstraint : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddCheckConstraint(
            name: "CK_cart_items_Quantity_Range",
            table: "cart_items",
            sql: "\"Quantity\" >= 1 AND \"Quantity\" <= 99");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_cart_items_Quantity_Range",
            table: "cart_items");
    }
}
