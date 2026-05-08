using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updatePayment_8_5_2026 : Migration
{
    private static readonly string[] columns = new[] { "id", "name" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "MONNIFY_CUSTOMER_ID",
            schema: "public",
            table: "TBL_USERS",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "STRIPE_CUSTOMER_ID",
            schema: "public",
            table: "TBL_USERS",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "customer_notes",
            schema: "public",
            table: "TBL_PAYMENTS",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "order_creation_failed",
            schema: "public",
            table: "TBL_PAYMENTS",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_PAYMENT_STATUS",
            columns: columns,
            values: new object[] { 7, "Disputed" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "public",
            table: "TBL_PAYMENT_STATUS",
            keyColumn: "id",
            keyValue: 7);

        migrationBuilder.DropColumn(
            name: "MONNIFY_CUSTOMER_ID",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "STRIPE_CUSTOMER_ID",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "customer_notes",
            schema: "public",
            table: "TBL_PAYMENTS");

        migrationBuilder.DropColumn(
            name: "order_creation_failed",
            schema: "public",
            table: "TBL_PAYMENTS");
    }
}
