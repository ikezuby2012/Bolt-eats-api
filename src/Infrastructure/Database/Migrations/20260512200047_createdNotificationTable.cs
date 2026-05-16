using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class createdNotificationTable : Migration
{
    private static readonly string[] columns = new[] { "id", "name" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TBL_NOTIFICATION_CHANNEL",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_notification_channel", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "TBL_NOTIFICATION_TYPE",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_notification_type", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "TBL_NOTIFICATION",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "GEN_RANDOM_UUID()"),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                notification_type_id = table.Column<int>(type: "integer", nullable: false),
                notification_channel_id = table.Column<int>(type: "integer", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                payload = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "text", nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_notification", x => x.id);
                table.ForeignKey(
                    name: "fk_tbl_notification_notification_type_notification_type_id",
                    column: x => x.notification_type_id,
                    principalSchema: "public",
                    principalTable: "TBL_NOTIFICATION_TYPE",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_tbl_notification_tbl_notification_channel_notification_chan",
                    column: x => x.notification_channel_id,
                    principalSchema: "public",
                    principalTable: "TBL_NOTIFICATION_CHANNEL",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_tbl_notification_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_NOTIFICATION_CHANNEL",
            columns: columns,
            values: new object[,]
            {
                { 1, "Push" },
                { 2, "InApp" },
                { 3, "Both" }
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_NOTIFICATION_TYPE",
            columns: columns,
            values: new object[,]
            {
                { 1, "Order Placed" },
                { 2, "Order Confirmed" },
                { 3, "Order Preparing" },
                { 4, "Order Ready for Pickup" },
                { 5, "Order Out for Delivery" },
                { 6, "Order Delivered" },
                { 7, "Order Cancelled" },
                { 8, "Payment Succeeded" },
                { 9, "Payment Failed" },
                { 10, "Promo Code Applied" },
                { 11, "Review Received" },
                { 12, "General" }
            });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_notification_notification_channel_id",
            schema: "public",
            table: "TBL_NOTIFICATION",
            column: "notification_channel_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_notification_notification_type_id",
            schema: "public",
            table: "TBL_NOTIFICATION",
            column: "notification_type_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_notification_user_id",
            schema: "public",
            table: "TBL_NOTIFICATION",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TBL_NOTIFICATION",
            schema: "public");

        migrationBuilder.DropTable(
            name: "TBL_NOTIFICATION_TYPE",
            schema: "public");

        migrationBuilder.DropTable(
            name: "TBL_NOTIFICATION_CHANNEL",
            schema: "public");
    }
}
