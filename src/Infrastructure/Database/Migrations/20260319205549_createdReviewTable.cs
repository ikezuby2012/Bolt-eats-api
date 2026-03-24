using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class createdReviewTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tbl_promo_codes",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "fixed"),
                discount_value = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                max_discount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                min_order_value = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                usage_limit = table.Column<int>(type: "integer", nullable: true),
                usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_promo_codes", x => x.id);
                table.CheckConstraint("ck_promo_code_discount_type", "lower(discount_type) IN ('fixed', 'percentage')");
                table.CheckConstraint("ck_promo_code_discount_value_positive", "discount_value > 0");
                table.CheckConstraint("ck_promo_code_expires_at_future", "expires_at > NOW() OR expires_at IS NULL");
                table.CheckConstraint("ck_promo_code_max_discount_not_less_than_value", "max_discount >= discount_value OR max_discount IS NULL OR discount_type = 'fixed'");
                table.CheckConstraint("ck_promo_code_max_discount_valid", "max_discount > 0 OR max_discount IS NULL");
                table.CheckConstraint("ck_promo_code_min_order_value_non_negative", "min_order_value >= 0");
                table.CheckConstraint("ck_promo_code_usage_count_valid", "usage_count >= 0 AND (usage_limit IS NULL OR usage_count <= usage_limit)");
                table.CheckConstraint("ck_promo_code_usage_limit_positive", "usage_limit > 0 OR usage_limit IS NULL");
            });

        migrationBuilder.CreateTable(
            name: "tbl_review",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                rating = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                comment = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_review", x => x.id);
                table.CheckConstraint("CK_review_rating", "rating >= 1 and rating <= 5");
                table.ForeignKey(
                    name: "fk_rating_order",
                    column: x => x.order_id,
                    principalSchema: "public",
                    principalTable: "tbl_order",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_rating_restaurant",
                    column: x => x.restaurant_id,
                    principalSchema: "public",
                    principalTable: "tbl_restaurant",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_rating_user",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "tbl_rider_location",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                rider_id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                latitude_raw = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                longitude_raw = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                bearing = table.Column<double>(type: "numeric(12,2)", nullable: false, defaultValue: 0.0),
                speed = table.Column<double>(type: "numeric(12,2)", nullable: false, defaultValue: 0.0),
                recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_rider_location", x => x.id);
                table.ForeignKey(
                    name: "fk_rider_location_order",
                    column: x => x.order_id,
                    principalSchema: "public",
                    principalTable: "tbl_order",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_rider_location_rider",
                    column: x => x.rider_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "x_tbl_promo_code_code_unique",
            schema: "public",
            table: "tbl_promo_codes",
            column: "code",
            unique: true,
            filter: "is_soft_deleted = false AND is_active = true");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_review_order_id",
            schema: "public",
            table: "tbl_review",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_review_restaurant_id",
            schema: "public",
            table: "tbl_review",
            column: "restaurant_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_review_user_id",
            schema: "public",
            table: "tbl_review",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_rider_location_order_id",
            schema: "public",
            table: "tbl_rider_location",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_rider_location_rider_id",
            schema: "public",
            table: "tbl_rider_location",
            column: "rider_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "tbl_promo_codes",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tbl_review",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tbl_rider_location",
            schema: "public");
    }
}
