using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class menuItem : Migration
{
    private static readonly string[] columns = new[] { "restaurant_id", "category_id" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tbl_category",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_category", x => x.id);
                table.CheckConstraint("CK_category_display_order_positive", "display_order > 0");
                table.ForeignKey(
                    name: "fk_category_restaurant",
                    column: x => x.restaurant_id,
                    principalSchema: "public",
                    principalTable: "tbl_restaurant",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "tbl_menu_item",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                discount_price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                calories = table.Column<int>(type: "integer", nullable: true),
                prep_time_min = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                is_available = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                is_popular = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_menu_item", x => x.id);
                table.CheckConstraint("CK_menu_item_discount_less_than_price", "discount_price < price OR discount_price IS NULL");
                table.CheckConstraint("CK_menu_item_discount_price_positive", "discount_price > 0 OR discount_price IS NULL");
                table.CheckConstraint("CK_menu_item_price_positive", "price > 0");
                table.ForeignKey(
                    name: "fk_menu_item_category",
                    column: x => x.category_id,
                    principalSchema: "public",
                    principalTable: "tbl_category",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_menu_item_restaurant",
                    column: x => x.restaurant_id,
                    principalSchema: "public",
                    principalTable: "tbl_restaurant",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_category_restaurant_id",
            schema: "public",
            table: "tbl_category",
            column: "restaurant_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_menu_item_category_id",
            schema: "public",
            table: "tbl_menu_item",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_menu_item_restaurant_category",
            schema: "public",
            table: "tbl_menu_item",
            columns: columns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "tbl_menu_item",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tbl_category",
            schema: "public");
    }
}
