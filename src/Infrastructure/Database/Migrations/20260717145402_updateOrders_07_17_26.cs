using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateOrders_07_17_26 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "contact_email",
            schema: "public",
            table: "tbl_order",
            type: "character varying(300)",
            maxLength: 300,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "contact_name",
            schema: "public",
            table: "tbl_order",
            type: "character varying(400)",
            maxLength: 400,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "contact_phone",
            schema: "public",
            table: "tbl_order",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "contact_email",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropColumn(
            name: "contact_name",
            schema: "public",
            table: "tbl_order");

        migrationBuilder.DropColumn(
            name: "contact_phone",
            schema: "public",
            table: "tbl_order");
    }
}
