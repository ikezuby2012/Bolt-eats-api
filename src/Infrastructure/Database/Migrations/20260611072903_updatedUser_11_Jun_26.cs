using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updatedUser_11_Jun_26 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "address_label",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<Dictionary<string, string>>(
            name: "building_details",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "jsonb",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "building_type",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "delivery_instructions",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "address_label",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.DropColumn(
            name: "building_details",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.DropColumn(
            name: "building_type",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.DropColumn(
            name: "delivery_instructions",
            schema: "public",
            table: "TBL_ADDRESS");
    }
}
