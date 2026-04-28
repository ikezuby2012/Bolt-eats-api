using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateDb_4_26_26 : Migration
{
    private static readonly string[] columns = new[] { "id", "name" };
    private static readonly string[] columnsArray = new[] { "promo_code_id", "user_id" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "ROLE_ID",
            schema: "public",
            table: "TBL_USERS",
            type: "integer",
            nullable: false,
            defaultValue: 1,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IS_ONLINE",
            schema: "public",
            table: "TBL_USERS",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AlterColumn<decimal>(
            name: "min_order_value",
            schema: "public",
            table: "tbl_promo_codes",
            type: "numeric(10,2)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(10,2)",
            oldDefaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "max_discount_cap",
            schema: "public",
            table: "tbl_promo_codes",
            type: "numeric(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "restaurant_id",
            schema: "public",
            table: "tbl_promo_codes",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "starts_at",
            schema: "public",
            table: "tbl_promo_codes",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "usage_limit_per_user",
            schema: "public",
            table: "tbl_promo_codes",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "order_id1",
            schema: "public",
            table: "tbl_order_item",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "cancellation_notes",
            schema: "public",
            table: "tbl_order",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "cancelled_at",
            schema: "public",
            table: "tbl_order",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "refunded_at",
            schema: "public",
            table: "tbl_order",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "promo_discount",
            schema: "public",
            table: "TBL_CART",
            type: "numeric(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "promo_discount_type",
            schema: "public",
            table: "TBL_CART",
            type: "text",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TBL_PROMO_USAGE_STATUS",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_promo_usage_status", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "promo_code_usages",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                promo_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                status_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                discount_applied = table.Column<decimal>(type: "numeric", nullable: false),
                redeemed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                times_used = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "text", nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_promo_code_usages", x => x.id);
                table.ForeignKey(
                    name: "fk_promo_code_usages_promo_usage_status_status_id",
                    column: x => x.status_id,
                    principalSchema: "public",
                    principalTable: "TBL_PROMO_USAGE_STATUS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_promo_code_usages_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_promo_usages",
                    column: x => x.promo_code_id,
                    principalSchema: "public",
                    principalTable: "tbl_promo_codes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_PROMO_USAGE_STATUS",
            columns: columns,
            values: new object[,]
            {
                { 1, "Pending" },
                { 2, "Redeemed" },
                { 3, "Cancelled" }
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_USER_ROLE",
            columns: columns,
            values: new object[] { 5, "Business_Owner" });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_promo_codes_restaurant_id",
            schema: "public",
            table: "tbl_promo_codes",
            column: "restaurant_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_item_order_id1",
            schema: "public",
            table: "tbl_order_item",
            column: "order_id1");

        migrationBuilder.CreateIndex(
            name: "ix_promo_code_usages_promo_code_id_user_id",
            schema: "public",
            table: "promo_code_usages",
            columns: columnsArray);

        migrationBuilder.CreateIndex(
            name: "ix_promo_code_usages_status_id",
            schema: "public",
            table: "promo_code_usages",
            column: "status_id");

        migrationBuilder.CreateIndex(
            name: "ix_promo_code_usages_user_id",
            schema: "public",
            table: "promo_code_usages",
            column: "user_id");

        migrationBuilder.AddForeignKey(
            name: "fk_tbl_order_item_tbl_order_order_id1",
            schema: "public",
            table: "tbl_order_item",
            column: "order_id1",
            principalSchema: "public",
            principalTable: "tbl_order",
            principalColumn: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_promo_restaurant",
            schema: "public",
            table: "tbl_promo_codes",
            column: "restaurant_id",
            principalSchema: "public",
            principalTable: "tbl_restaurant",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_tbl_order_item_tbl_order_order_id1",
            schema: "public",
            table: "tbl_order_item");

        migrationBuilder.DropForeignKey(
            name: "fk_promo_restaurant",
            schema: "public",
            table: "tbl_promo_codes");

        migrationBuilder.DropTable(
            name: "promo_code_usages",
            schema: "public");

        migrationBuilder.DropTable(
            name: "TBL_PROMO_USAGE_STATUS",
            schema: "public");

        migrationBuilder.DropIndex(
            name: "ix_tbl_promo_codes_restaurant_id",
            schema: "public",
            table: "tbl_promo_codes");

        migrationBuilder.DropIndex(
            name: "ix_tbl_order_item_order_id1",
            schema: "public",
            table: "tbl_order_item");

        migrationBuilder.DeleteData(
            schema: "public",
            table: "TBL_USER_ROLE",
            keyColumn: "id",
            keyValue: 5);

        migrationBuilder.DropColumn(
            name: "IS_ONLINE",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "max_discount_cap",
            schema: "public",
            table: "tbl_promo_codes");

        migrationBuilder.DropColumn(
            name: "restaurant_id",
            schema: "public",
            table: "tbl_promo_codes");

        migrationBuilder.DropColumn(
            name: "starts_at",
            schema: "public",
            table: "tbl_promo_codes");

        migrationBuilder.DropColumn(
            name: "usage_limit_per_user",
            schema: "public",
            table: "tbl_promo_codes");

        migrationBuilder.DropColumn(
            name: "order_id1",
            schema: "public",
            table: "tbl_order_item");

        migrationBuilder.DropColumn(
            name: "cancellation_notes",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropColumn(
            name: "cancelled_at",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropColumn(
            name: "refunded_at",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropColumn(
            name: "promo_discount",
            schema: "public",
            table: "TBL_CART");

        migrationBuilder.DropColumn(
            name: "promo_discount_type",
            schema: "public",
            table: "TBL_CART");

        migrationBuilder.AlterColumn<int>(
            name: "ROLE_ID",
            schema: "public",
            table: "TBL_USERS",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer",
            oldDefaultValue: 1);

        migrationBuilder.AlterColumn<decimal>(
            name: "min_order_value",
            schema: "public",
            table: "tbl_promo_codes",
            type: "numeric(10,2)",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric(10,2)",
            oldNullable: true);
    }
}
