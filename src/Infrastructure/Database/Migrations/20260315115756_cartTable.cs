using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class cartTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TBL_CART",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                promo_code = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_cart", x => x.id);
                table.ForeignKey(
                    name: "fk_cart_restaurant",
                    column: x => x.restaurant_id,
                    principalSchema: "public",
                    principalTable: "tbl_restaurant",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_cart_user",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TBL_CART_ITEM",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                menu_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                unit_price = table.Column<decimal>(type: "numeric", nullable: false),
                notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_cart_item", x => x.id);
                table.CheckConstraint("CK_cart_item_unit_price_positive", "unit_price > 0");
                table.ForeignKey(
                    name: "fk_cart_item_cart",
                    column: x => x.cart_id,
                    principalSchema: "public",
                    principalTable: "TBL_CART",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_cart_item_menu_item",
                    column: x => x.menu_item_id,
                    principalSchema: "public",
                    principalTable: "tbl_menu_item",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_cart_restaurant_id",
            schema: "public",
            table: "TBL_CART",
            column: "restaurant_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_cart_user_id",
            schema: "public",
            table: "TBL_CART",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_cart_item_cart_id",
            schema: "public",
            table: "TBL_CART_ITEM",
            column: "cart_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_cart_item_menu_item_id",
            schema: "public",
            table: "TBL_CART_ITEM",
            column: "menu_item_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TBL_CART_ITEM",
            schema: "public");

        migrationBuilder.DropTable(
            name: "TBL_CART",
            schema: "public");
    }
}
