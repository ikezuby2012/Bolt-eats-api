using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class createResaurantTable : Migration
{
    private static readonly string[] columns = new[] { "is_active", "is_open" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tbl_restaurant",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                banner_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                address_id = table.Column<Guid>(type: "uuid", nullable: true),
                rating = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                total_reviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                delivery_fee_min = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                delivery_fee_max = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                min_order_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                est_delivery_min = table.Column<int>(type: "integer", nullable: true),
                est_delivery_max = table.Column<int>(type: "integer", nullable: true),
                is_open = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                uber_one_partner = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_restaurant", x => x.id);
                table.CheckConstraint("CK_restaurant_rating_range", "rating BETWEEN 0 AND 5");
                table.CheckConstraint("CK_restaurant_total_reviews_non_negative", "total_reviews >= 0");
                table.ForeignKey(
                    name: "fk_restaurant_address",
                    column: x => x.address_id,
                    principalSchema: "public",
                    principalTable: "TBL_ADDRESS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_restaurant_owner",
                    column: x => x.owner_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_restaurant_address_id",
            schema: "public",
            table: "tbl_restaurant",
            column: "address_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_restaurant_owner_id",
            schema: "public",
            table: "tbl_restaurant",
            column: "owner_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_restaurant_rating",
            schema: "public",
            table: "tbl_restaurant",
            column: "rating",
            filter: "is_soft_deleted = false AND is_active = true");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_restaurant_status",
            schema: "public",
            table: "tbl_restaurant",
            columns: columns,
            filter: "is_soft_deleted = false");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "tbl_restaurant",
            schema: "public");
    }
}
