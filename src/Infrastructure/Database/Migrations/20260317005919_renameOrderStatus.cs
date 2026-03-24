using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class renameOrderStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(
            name: "TBL_ORDER_STATUS",
            schema: "public",
            newName: "tbl_order_status",
            newSchema: "public");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(
            name: "tbl_order_status",
            schema: "public",
            newName: "TBL_ORDER_STATUS",
            newSchema: "public");
    }
}
