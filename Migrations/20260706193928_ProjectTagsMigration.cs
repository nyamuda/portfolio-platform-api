using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectTagsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TechStack",
                table: "Projects",
                newName: "Tags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "Projects",
                newName: "TechStack");
        }
    }
}
