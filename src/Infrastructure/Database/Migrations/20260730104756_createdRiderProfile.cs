using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class createdRiderProfile : Migration
{
    private static readonly string[] columns = new[] { "id", "name" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TBL_RIDER_VERIFICATION_STATUS",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_rider_verification_status", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "TBL_RIDER_PROFILE",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                number_plate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                vehicle_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                vehicle_make = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                vehicle_model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                vehicle_color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                vehicle_year = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                driver_license_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                driver_license_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                national_id_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                national_id_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                vehicle_photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                vehicle_photo_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                insurance_cert_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                insurance_cert_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                status_id = table.Column<int>(type: "integer", nullable: false),
                rejection_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                verified_by = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "text", nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_rider_profile", x => x.id);
                table.CheckConstraint("CK_RIDER_PROFILE_VEHICLE_TYPE", "\"vehicle_type\" IN ('Motorcycle', 'Bicycle', 'Car')");
                table.ForeignKey(
                    name: "fk_tbl_rider_profile_rider_verification_status_status_id",
                    column: x => x.status_id,
                    principalSchema: "public",
                    principalTable: "TBL_RIDER_VERIFICATION_STATUS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_tbl_rider_profile_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_RIDER_VERIFICATION_STATUS",
            columns: columns,
            values: new object[,]
            {
                { 1, "Pending" },
                { 2, "Under Review" },
                { 3, "Succeeded" },
                { 4, "Rejected" }
            });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_rider_profile_number_plate",
            schema: "public",
            table: "TBL_RIDER_PROFILE",
            column: "number_plate",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tbl_rider_profile_status_id",
            schema: "public",
            table: "TBL_RIDER_PROFILE",
            column: "status_id");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_rider_profile_user_id",
            schema: "public",
            table: "TBL_RIDER_PROFILE",
            column: "user_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TBL_RIDER_PROFILE",
            schema: "public");

        migrationBuilder.DropTable(
            name: "TBL_RIDER_VERIFICATION_STATUS",
            schema: "public");
    }
}
