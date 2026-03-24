using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class createOrdersTable : Migration
{
    private static readonly string[] columns = new[] { "id", "description", "name" };
    private static readonly string[] columnsArray = new[] { "id", "name" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TBL_ORDER_STATUS",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_order_status", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tbl_order",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                rider_id = table.Column<Guid>(type: "uuid", nullable: true),
                address_id = table.Column<Guid>(type: "uuid", nullable: false),
                order_status_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                subtotal = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                delivery_fee = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                discount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                tax = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                total = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                promo_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                payment_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                checkout_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_order", x => x.id);
                table.CheckConstraint("CK_order_accepted_before_pickedup", "accepted_at <= picked_up_at OR picked_up_at IS NULL");
                table.CheckConstraint("CK_order_checkout_before_accepted", "checkout_at <= accepted_at OR accepted_at IS NULL");
                table.CheckConstraint("CK_order_delivery_fee_positive", "delivery_fee >= 0");
                table.CheckConstraint("CK_order_discount_not_exceed_subtotal", "discount <= subtotal OR discount IS NULL");
                table.CheckConstraint("CK_order_discount_positive", "discount >= 0 OR discount IS NULL");
                table.CheckConstraint("CK_order_pickedup_before_delivered", "picked_up_at <= delivered_at OR delivered_at IS NULL");
                table.CheckConstraint("CK_order_subtotal_positive", "subtotal >= 0");
                table.CheckConstraint("CK_order_tax_positive", "tax >= 0");
                table.CheckConstraint("CK_order_total_calculation", "total = subtotal + delivery_fee + tax - COALESCE(discount, 0)");
                table.CheckConstraint("CK_order_total_positive", "total >= 0");
                table.ForeignKey(
                    name: "fk_order_address",
                    column: x => x.address_id,
                    principalSchema: "public",
                    principalTable: "TBL_ADDRESS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_order_customer",
                    column: x => x.customer_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_order_restaurant",
                    column: x => x.restaurant_id,
                    principalSchema: "public",
                    principalTable: "tbl_restaurant",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_order_rider",
                    column: x => x.rider_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_order_status",
                    column: x => x.order_status_id,
                    principalSchema: "public",
                    principalTable: "TBL_ORDER_STATUS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "tbl_order_item",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                menu_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                unit_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_order_item", x => x.id);
                table.CheckConstraint("CK_order_item_quantity_positive", "quantity > 0");
                table.CheckConstraint("CK_order_item_total_positive", "quantity * unit_price > 0");
                table.CheckConstraint("CK_order_item_unit_price_positive", "unit_price > 0");
                table.ForeignKey(
                    name: "fk_order_item_menu_item",
                    column: x => x.menu_item_id,
                    principalSchema: "public",
                    principalTable: "tbl_menu_item",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_order_item_order",
                    column: x => x.order_id,
                    principalSchema: "public",
                    principalTable: "tbl_order",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_ORDER_STATUS",
            columns: columns,
            values: new object[,]
            {
                { 1, "Order placed, awaiting restaurant acceptance", "Pending" },
                { 2, "Restaurant confirmed the order", "Accepted" },
                { 3, "Food is being prepared", "Preparing" },
                { 4, "Ready for rider collection", "Ready_For_Pickup" },
                { 5, "Rider en route to customer", "In_Transit" },
                { 6, "Order completed Successfully", "Delivered" },
                { 7, "Cancelled by customer or restaurant", "Cancelled" },
                { 8, "Payment reversed", "Refunded" }
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_USER_ROLE",
            columns: columnsArray,
            values: new object[] { 4, "Rider" });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_address_id",
            schema: "public",
            table: "tbl_order",
            column: "address_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_customer_id",
            schema: "public",
            table: "tbl_order",
            column: "customer_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_order_status_id",
            schema: "public",
            table: "tbl_order",
            column: "order_status_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_restaurant_id",
            schema: "public",
            table: "tbl_order",
            column: "restaurant_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_rider_id",
            schema: "public",
            table: "tbl_order",
            column: "rider_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_item_menu_item_id",
            schema: "public",
            table: "tbl_order_item",
            column: "menu_item_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_order_item_order_id",
            schema: "public",
            table: "tbl_order_item",
            column: "order_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "tbl_order_item",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tbl_order",
            schema: "public");

        migrationBuilder.DropTable(
            name: "TBL_ORDER_STATUS",
            schema: "public");

        migrationBuilder.DeleteData(
            schema: "public",
            table: "TBL_USER_ROLE",
            keyColumn: "id",
            keyValue: 4);
    }
}
