using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobRequest.Shared.Migrations
{
    /// <inheritdoc />
    public partial class FixBrokenAuthData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear the Auth column if it contains non-JSON data (e.g. from the previous broken migration's RenameColumn)
            migrationBuilder.Sql("UPDATE Environments SET Auth = '{}' WHERE Auth IS NOT NULL AND Auth NOT LIKE '{%'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
