using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobRequest.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoryItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Method = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseTimeMs = table.Column<long>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Request = table.Column<string>(type: "TEXT", nullable: true),
                    Response = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DarkMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultTimeoutSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxHistoryItems = table.Column<int>(type: "INTEGER", nullable: false),
                    AutoFormatJson = table.Column<bool>(type: "INTEGER", nullable: false),
                    FollowRedirects = table.Column<bool>(type: "INTEGER", nullable: false),
                    ValidateSslCertificates = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActiveEnvironmentId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryItems_Timestamp",
                table: "HistoryItems",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryItems_Url",
                table: "HistoryItems",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryItems");

            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
