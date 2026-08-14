using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updOrder_20_7_2026 : Migration
{
    private static readonly string[] columns = new[] { "description", "name" };
    private static readonly string[] columnsArray = new[] { "id", "description", "name" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "order_code",
            schema: "public",
            table: "tbl_order",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "");

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 1,
            columns: columns,
            values: new object[] { "Order created, waiting for customer payment", "Awaiting_Payment" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 2,
            columns: columns,
            values: new object[] { "Payment received, awaiting restaurant acceptance", "Pending" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 3,
            columns: columns,
            values: new object[] { "Restaurant confirmed the order", "Accepted" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 4,
            columns: columns,
            values: new object[] { "Food is being prepared", "Preparing" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 5,
            columns: columns,
            values: new object[] { "Ready for rider collection", "Ready_For_Pickup" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 6,
            columns: columns,
            values: new object[] { "Rider en route to customer", "In_Transit" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 7,
            columns: columns,
            values: new object[] { "Order completed successfully", "Delivered" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 8,
            columns: columns,
            values: new object[] { "Cancelled by customer or restaurant", "Cancelled" });

        migrationBuilder.InsertData(
            schema: "public",
            table: "tbl_order_status",
            columns: columnsArray,
            values: new object[] { 9, "Payment reversed", "Refunded" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 9);

        migrationBuilder.DropColumn(
            name: "order_code",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 1,
            columns: columns,
            values: new object[] { "Order placed, awaiting restaurant acceptance", "Pending" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 2,
            columns: columns,
            values: new object[] { "Restaurant confirmed the order", "Accepted" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 3,
            columns: columns,
            values: new object[] { "Food is being prepared", "Preparing" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 4,
            columns: columns,
            values: new object[] { "Ready for rider collection", "Ready_For_Pickup" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 5,
            columns: columns,
            values: new object[] { "Rider en route to customer", "In_Transit" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 6,
            columns: columns,
            values: new object[] { "Order completed Successfully", "Delivered" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 7,
            columns: columns,
            values: new object[] { "Cancelled by customer or restaurant", "Cancelled" });

        migrationBuilder.UpdateData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 8,
            columns: columns,
            values: new object[] { "Payment reversed", "Refunded" });
    }
}
