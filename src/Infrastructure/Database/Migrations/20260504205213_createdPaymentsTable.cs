using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class createdPaymentsTable : Migration
{
    private static readonly string[] columns = new[] { "id", "name" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TBL_PAYMENT_GATEWAY",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_payment_gateway", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "TBL_PAYMENT_STATUS",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_payment_status", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "TBL_PAYMENTS",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                gateway_id = table.Column<int>(type: "integer", nullable: false),
                status_id = table.Column<int>(type: "integer", nullable: false),
                amount = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                amount_in_kobo = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: false),
                gateway_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                client_secret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                gateway_customer_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                failure_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                failure_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                refund_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                refund_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                refunded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "text", nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_payments", x => x.id);
                table.ForeignKey(
                    name: "fk_tbl_payments_payment_gateway_gateway_id",
                    column: x => x.gateway_id,
                    principalSchema: "public",
                    principalTable: "TBL_PAYMENT_GATEWAY",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_tbl_payments_payment_status_status_id",
                    column: x => x.status_id,
                    principalSchema: "public",
                    principalTable: "TBL_PAYMENT_STATUS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_tbl_payments_tbl_order_order_id",
                    column: x => x.order_id,
                    principalSchema: "public",
                    principalTable: "tbl_order",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_tbl_payments_users_customer_id",
                    column: x => x.customer_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_PAYMENT_GATEWAY",
            columns: columns,
            values: new object[,]
            {
                { 1, "Stripe" },
                { 2, "Monnify" }
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_PAYMENT_STATUS",
            columns: columns,
            values: new object[,]
            {
                { 1, "Pending" },
                { 2, "Processing" },
                { 3, "Succeeded" },
                { 4, "Failed" },
                { 5, "Refunded" },
                { 6, "Partial Refund" }
            });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_payments_customer_id",
            schema: "public",
            table: "TBL_PAYMENTS",
            column: "customer_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_payments_gateway_id",
            schema: "public",
            table: "TBL_PAYMENTS",
            column: "gateway_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_payments_gateway_reference",
            schema: "public",
            table: "TBL_PAYMENTS",
            column: "gateway_reference",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tbl_payments_order_id",
            schema: "public",
            table: "TBL_PAYMENTS",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_payments_status_id",
            schema: "public",
            table: "TBL_PAYMENTS",
            column: "status_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TBL_PAYMENTS",
            schema: "public");

        migrationBuilder.DropTable(
            name: "TBL_PAYMENT_GATEWAY",
            schema: "public");

        migrationBuilder.DropTable(
            name: "TBL_PAYMENT_STATUS",
            schema: "public");
    }
}
