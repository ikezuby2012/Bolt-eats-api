using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class whatChanged : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {

    }
    private static readonly string[] columns = new[] { "is_revoked", "is_used", "expires_at" };

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_tbl_refresh_token_active",
            schema: "public",
            table: "tbl_refresh_token",
            columns: columns,
            filter: "is_soft_deleted = false AND is_revoked = false AND is_used = false AND expires_at > NOW()");
    }
}
