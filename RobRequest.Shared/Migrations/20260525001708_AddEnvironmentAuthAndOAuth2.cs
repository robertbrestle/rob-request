using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobRequest.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddEnvironmentAuthAndOAuth2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiKeyLocation",
                table: "Environments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyName",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyValue",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthPassword",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthToken",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AuthType",
                table: "Environments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AuthUsername",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OAuth2AutoRefresh",
                table: "Environments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OAuth2ClientId",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OAuth2ClientSecret",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OAuth2GrantType",
                table: "Environments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OAuth2Scope",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OAuth2TokenExpiresAt",
                table: "Environments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuth2TokenUrl",
                table: "Environments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKeyLocation",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "ApiKeyName",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "ApiKeyValue",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "AuthPassword",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "AuthToken",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "AuthType",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "AuthUsername",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "OAuth2AutoRefresh",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "OAuth2ClientId",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "OAuth2ClientSecret",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "OAuth2GrantType",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "OAuth2Scope",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "OAuth2TokenExpiresAt",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "OAuth2TokenUrl",
                table: "Environments");
        }
    }
}
