using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameAvatarUrlToProfileImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep the existing uploaded image URLs while moving to clearer profile terminology.
            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "Profiles",
                newName: "ProfileImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the column name only; the image URLs themselves remain untouched.
            migrationBuilder.RenameColumn(
                name: "ProfileImageUrl",
                table: "Profiles",
                newName: "AvatarUrl");
        }
    }
}