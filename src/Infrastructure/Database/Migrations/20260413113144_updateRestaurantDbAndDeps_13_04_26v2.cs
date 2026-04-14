using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class updateRestaurantDbAndDeps_13_04_26v2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        //migrationBuilder.DropIndex(
        //    name: "ix_tbl_address_location",
        //    schema: "public",
        //    table: "TBL_ADDRESS");

        migrationBuilder.AlterColumn<Point>(
            name: "location",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "geography (point, 4326)",
            nullable: true,
            oldClrType: typeof(Point),
            oldType: "geography",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_tbl_address_location",
            schema: "public",
            table: "TBL_ADDRESS",
            column: "location")
            .Annotation("Npgsql:IndexMethod", "GIST");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_tbl_address_location",
            schema: "public",
            table: "TBL_ADDRESS");

        migrationBuilder.AlterColumn<Point>(
            name: "location",
            schema: "public",
            table: "TBL_ADDRESS",
            type: "geography",
            nullable: true,
            oldClrType: typeof(Point),
            oldType: "geography (point, 4326)",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_tbl_address_location",
            schema: "public",
            table: "TBL_ADDRESS",
            column: "location")
            .Annotation("Npgsql:IndexMethod", "SPATIAL");
    }
}
