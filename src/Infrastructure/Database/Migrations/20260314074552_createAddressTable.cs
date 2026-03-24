using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class createAddressTable : Migration
{
    private static readonly string[] columns = new[] { "user_id", "is_default" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TBL_ADDRESS",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                latitude_raw = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                longitude_raw = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_address", x => x.id);
                table.CheckConstraint("CK_address_latitude_range", "latitude BETWEEN -90 AND 90");
                table.CheckConstraint("CK_address_longitude_range", "longitude BETWEEN -180 AND 180");
                table.ForeignKey(
                    name: "fk_address_user",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "TBL_USERS",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_address_user_default",
            schema: "public",
            table: "TBL_ADDRESS",
            columns: columns,
            unique: true,
            filter: "is_soft_deleted = false");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_address_user_id",
            schema: "public",
            table: "TBL_ADDRESS",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TBL_ADDRESS",
            schema: "public");
    }
}
