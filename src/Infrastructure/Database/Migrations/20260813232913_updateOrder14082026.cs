using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateOrder14082026 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "offered_to_rider_id",
            schema: "public",
            table: "tbl_order",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_offered_to_rider_id",
            schema: "public",
            table: "tbl_order",
            column: "offered_to_rider_id");

        migrationBuilder.AddForeignKey(
            name: "fk_order_offered_to_rider",
            schema: "public",
            table: "tbl_order",
            column: "offered_to_rider_id",
            principalSchema: "public",
            principalTable: "TBL_USERS",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_order_offered_to_rider",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropIndex(
            name: "ix_tbl_order_offered_to_rider_id",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropColumn(
            name: "offered_to_rider_id",
            schema: "public",
            table: "tbl_order");
    }
}
