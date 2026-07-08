using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProfileContentFieldsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep the existing short profile summary. We only remove the old length limit
            // because Bio is still separate from the full About content pair.
            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "Profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            // Preserve existing focus text while moving it to the clearer CurrentFocus name.
            migrationBuilder.RenameColumn(
                name: "Focus",
                table: "Profiles",
                newName: "CurrentFocus");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentFocus",
                table: "Profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AboutContentHtml",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AboutContentText",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "Profiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AboutContentHtml",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AboutContentText",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "Profiles");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentFocus",
                table: "Profiles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "CurrentFocus",
                table: "Profiles",
                newName: "Focus");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "Profiles",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
