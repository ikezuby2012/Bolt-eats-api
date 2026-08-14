using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateOrderStatus_26_07_25 : Migration
{
    private static readonly string[] columns = new[] { "id", "description", "name" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            schema: "public",
            table: "tbl_order_status",
            columns: columns,
            values: new object[] { 10, "Payment failed", "Payment_Failed" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "public",
            table: "tbl_order_status",
            keyColumn: "id",
            keyValue: 10);
    }
}
