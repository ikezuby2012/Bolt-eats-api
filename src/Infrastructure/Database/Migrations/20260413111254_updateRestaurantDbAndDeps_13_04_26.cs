using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateRestaurantDbAndDeps_13_04_26 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_restaurant_address",
            schema: "public",
            table: "tbl_restaurant");

        migrationBuilder.DropIndex(
            name: "ix_tbl_restaurant_address_id",
            schema: "public",
            table: "tbl_restaurant");

        migrationBuilder.DropColumn(
            name: "address_id",
            schema: "public",
            table: "tbl_restaurant");

        migrationBuilder.RenameColumn(
            name: "uber_one_partner",
            schema: "public",
            table: "tbl_restaurant",
            newName: "company_partner");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:PostgresExtension:postgis", ",,");

        migrationBuilder.AddColumn<string>(
            name: "name",
            schema: "public",
            table: "tbl_category",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AlterColumn<Guid>(
            name: "user_id",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AddColumn<Point>(
            name: "location",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "geography",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "restaurant_id",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "uuid",
            nullable: true);

        //migrationBuilder.CreateIndex(
        //    name: "ix_tbl_address_location",
        //    schema: "public",
        //    table: "TBL_ADDRESS",
        //    column: "location")
        //    .Annotation("Npgsql:IndexMethod", "SPATIAL");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_address_restaurant_id",
            schema: "public",
            table: "TBL_ADDRESS",
            column: "restaurant_id");

        migrationBuilder.AddForeignKey(
            name: "fk_restaurant_address",
            schema: "public",
            table: "TBL_ADDRESS",
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
            name: "fk_restaurant_address",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.DropIndex(
            name: "ix_tbl_address_location",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.DropIndex(
            name: "ix_tbl_address_restaurant_id",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.DropColumn(
            name: "name",
            schema: "public",
            table: "tbl_category");

        migrationBuilder.DropColumn(
            name: "location",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.DropColumn(
            name: "restaurant_id",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.RenameColumn(
            name: "company_partner",
            schema: "public",
            table: "tbl_restaurant",
            newName: "uber_one_partner");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

        migrationBuilder.AddColumn<Guid>(
            name: "address_id",
            schema: "public",
            table: "tbl_restaurant",
            type: "uuid",
            nullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "user_id",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_tbl_restaurant_address_id",
            schema: "public",
            table: "tbl_restaurant",
            column: "address_id");

        migrationBuilder.AddForeignKey(
            name: "fk_restaurant_address",
            schema: "public",
            table: "tbl_restaurant",
            column: "address_id",
            principalSchema: "public",
            principalTable: "TBL_ADDRESS",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }
}
