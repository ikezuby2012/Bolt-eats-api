using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class createdRefreshTokenTable : Migration
{

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tbl_refresh_token",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                created_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                revoked_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_soft_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_refresh_token", x => x.id);
            });


        migrationBuilder.CreateIndex(
            name: "ix_tbl_refresh_token_created_by_ip",
            schema: "public",
            table: "tbl_refresh_token",
            column: "created_by_ip",
            filter: "is_soft_deleted = false AND created_by_ip IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_refresh_token_expires_at",
            schema: "public",
            table: "tbl_refresh_token",
            column: "expires_at",
            filter: "is_soft_deleted = false AND is_revoked = false");

        migrationBuilder.CreateIndex(
            name: "ix_tbl_refresh_token_token_unique",
            schema: "public",
            table: "tbl_refresh_token",
            column: "token",
            unique: true,
            filter: "is_soft_deleted = false");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "tbl_refresh_token",
            schema: "public");
    }
}
