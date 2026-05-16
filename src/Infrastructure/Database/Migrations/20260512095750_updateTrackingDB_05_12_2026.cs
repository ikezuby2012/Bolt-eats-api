using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateTrackingDB_05_12_2026 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<double>(
            name: "accuracy",
            schema: "public",
            table: "tbl_rider_location",
            type: "double precision",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "estimated_delivery_minutes",
            schema: "public",
            table: "tbl_order",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "estimated_travel_minutes",
            schema: "public",
            table: "tbl_order",
            type: "integer",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "accuracy",
            schema: "public",
            table: "tbl_rider_location");

        migrationBuilder.DropColumn(
            name: "estimated_delivery_minutes",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropColumn(
            name: "estimated_travel_minutes",
            schema: "public",
            table: "tbl_order");
    }
}
