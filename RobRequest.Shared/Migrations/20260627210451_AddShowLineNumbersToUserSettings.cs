using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobRequest.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddShowLineNumbersToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowLineNumbers",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowLineNumbers",
                table: "UserSettings");
        }
    }
}
