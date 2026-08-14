using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateOrder_23_07_25 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "cart_id",
            schema: "public",
            table: "tbl_order",
            type: "uuid",
            nullable: false);

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_cart_id",
            schema: "public",
            table: "tbl_order",
            column: "cart_id");

        migrationBuilder.AddForeignKey(
            name: "fk_order_cart",
            schema: "public",
            table: "tbl_order",
            column: "cart_id",
            principalSchema: "public",
            principalTable: "TBL_CART",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_order_cart",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropIndex(
            name: "ix_tbl_order_cart_id",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropColumn(
            name: "cart_id",
            schema: "public",
            table: "tbl_order");
    }
}
