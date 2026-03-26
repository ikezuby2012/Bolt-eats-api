using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateUser_3_25 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_address_user",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.AddColumn<DateTime>(
            name: "date_of_birth",
            schema: "public",
            table: "TBL_USERS",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "phone_number",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "profile_image_url",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddForeignKey(
            name: "fk_user_addresses",
            schema: "public",
            table: "TBL_ADDRESS",
            column: "user_id",
            principalSchema: "public",
            principalTable: "TBL_USERS",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_user_addresses",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.DropColumn(
            name: "date_of_birth",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "phone_number",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "profile_image_url",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.AddForeignKey(
            name: "fk_address_user",
            schema: "public",
            table: "TBL_ADDRESS",
            column: "user_id",
            principalSchema: "public",
            principalTable: "TBL_USERS",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }
}
