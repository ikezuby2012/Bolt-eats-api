using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class FixUserTable : Migration
{
    private static readonly string[] columns = new[] { "id", "name" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_users",
            schema: "public",
            table: "users");

        migrationBuilder.RenameTable(
            name: "users",
            schema: "public",
            newName: "TBL_USERS",
            newSchema: "public");

        migrationBuilder.RenameColumn(
            name: "password_hash",
            schema: "public",
            table: "TBL_USERS",
            newName: "PASSWORD_HASH");

        migrationBuilder.RenameColumn(
            name: "last_name",
            schema: "public",
            table: "TBL_USERS",
            newName: "LAST_NAME");

        migrationBuilder.RenameColumn(
            name: "first_name",
            schema: "public",
            table: "TBL_USERS",
            newName: "FIRST_NAME");

        migrationBuilder.RenameColumn(
            name: "email",
            schema: "public",
            table: "TBL_USERS",
            newName: "EMAIL");

        migrationBuilder.RenameIndex(
            name: "ix_users_email",
            schema: "public",
            table: "TBL_USERS",
            newName: "ix_tbl_users_email");

        migrationBuilder.AlterColumn<string>(
            name: "PASSWORD_HASH",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(512)",
            maxLength: 512,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "LAST_NAME",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "FIRST_NAME",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "EMAIL",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(256)",
            maxLength: 256,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AddColumn<DateTime>(
            name: "CREATED_AT",
            schema: "public",
            table: "TBL_USERS",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CREATED_BY",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IS_ACTIVE",
            schema: "public",
            table: "TBL_USERS",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<bool>(
            name: "IS_SOFT_DELETED",
            schema: "public",
            table: "TBL_USERS",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IS_VERIFIED",
            schema: "public",
            table: "TBL_USERS",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "LAST_LOGIN",
            schema: "public",
            table: "TBL_USERS",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OTP",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(6)",
            maxLength: 6,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "ROLE_ID",
            schema: "public",
            table: "TBL_USERS",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "UPDATED_AT",
            schema: "public",
            table: "TBL_USERS",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UPDATED_BY",
            schema: "public",
            table: "TBL_USERS",
            type: "character varying(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "created_by_id",
            schema: "public",
            table: "TBL_USERS",
            type: "text",
            nullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "pk_tbl_users",
            schema: "public",
            table: "TBL_USERS",
            column: "id");

        migrationBuilder.CreateTable(
            name: "TBL_USER_ROLE",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tbl_user_role", x => x.id);
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "TBL_USER_ROLE",
            columns: columns,
            values: new object[,]
            {
                { 1, "User" },
                { 2, "Business_Developer" },
                { 3, "Admin" }
            });

        migrationBuilder.CreateIndex(
            name: "ix_tbl_users_role_id",
            schema: "public",
            table: "TBL_USERS",
            column: "ROLE_ID");

        migrationBuilder.AddForeignKey(
            name: "fk_tbl_users_user_role_role_id",
            schema: "public",
            table: "TBL_USERS",
            column: "ROLE_ID",
            principalSchema: "public",
            principalTable: "TBL_USER_ROLE",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_tbl_users_user_role_role_id",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropTable(
            name: "TBL_USER_ROLE",
            schema: "public");

        migrationBuilder.DropPrimaryKey(
            name: "pk_tbl_users",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropIndex(
            name: "ix_tbl_users_role_id",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "CREATED_AT",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "CREATED_BY",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "IS_ACTIVE",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "IS_SOFT_DELETED",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "IS_VERIFIED",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "LAST_LOGIN",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "OTP",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "ROLE_ID",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "UPDATED_AT",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "UPDATED_BY",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.DropColumn(
            name: "created_by_id",
            schema: "public",
            table: "TBL_USERS");

        migrationBuilder.RenameTable(
            name: "TBL_USERS",
            schema: "public",
            newName: "users",
            newSchema: "public");

        migrationBuilder.RenameColumn(
            name: "PASSWORD_HASH",
            schema: "public",
            table: "users",
            newName: "password_hash");

        migrationBuilder.RenameColumn(
            name: "LAST_NAME",
            schema: "public",
            table: "users",
            newName: "last_name");

        migrationBuilder.RenameColumn(
            name: "FIRST_NAME",
            schema: "public",
            table: "users",
            newName: "first_name");

        migrationBuilder.RenameColumn(
            name: "EMAIL",
            schema: "public",
            table: "users",
            newName: "email");

        migrationBuilder.RenameIndex(
            name: "ix_tbl_users_email",
            schema: "public",
            table: "users",
            newName: "ix_users_email");

        migrationBuilder.AlterColumn<string>(
            name: "password_hash",
            schema: "public",
            table: "users",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(512)",
            oldMaxLength: 512);

        migrationBuilder.AlterColumn<string>(
            name: "last_name",
            schema: "public",
            table: "users",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "first_name",
            schema: "public",
            table: "users",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "email",
            schema: "public",
            table: "users",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(256)",
            oldMaxLength: 256);

        migrationBuilder.AddPrimaryKey(
            name: "pk_users",
            schema: "public",
            table: "users",
            column: "id");
    }
}
