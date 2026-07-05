using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cafe1316.Infrastructure.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260703010000_AddPaymentIdempotencyConstraints")]
public partial class AddPaymentIdempotencyConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_orders_StripePaymentIntentId",
            table: "orders",
            column: "StripePaymentIntentId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_payments_TransactionId",
            table: "payments",
            column: "TransactionId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_orders_StripePaymentIntentId",
            table: "orders");

        migrationBuilder.DropIndex(
            name: "IX_payments_TransactionId",
            table: "payments");
    }
}
