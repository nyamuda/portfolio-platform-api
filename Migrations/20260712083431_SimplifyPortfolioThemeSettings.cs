using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PortfolioPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyPortfolioThemeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileThemeSettings");

            migrationBuilder.DropTable(
                name: "PortfolioThemes");

            migrationBuilder.AddColumn<int>(
                name: "Theme",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ThemeAccentColor",
                table: "Profiles",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#1640D6");

            migrationBuilder.AddColumn<int>(
                name: "ThemeTypography",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ThemeAccentColor",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ThemeTypography",
                table: "Profiles");

            migrationBuilder.CreateTable(
                name: "PortfolioThemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Availability = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    IsFallback = table.Column<bool>(type: "boolean", nullable: false),
                    Key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    LatestVersion = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PreviewImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioThemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileThemeSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActiveThemeId = table.Column<int>(type: "integer", nullable: false),
                    DraftThemeId = table.Column<int>(type: "integer", nullable: true),
                    ProfileId = table.Column<int>(type: "integer", nullable: false),
                    ActiveSettingsJson = table.Column<string>(type: "text", nullable: false),
                    ActiveThemeVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DraftSettingsJson = table.Column<string>(type: "text", nullable: true),
                    DraftThemeVersion = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileThemeSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileThemeSettings_PortfolioThemes_ActiveThemeId",
                        column: x => x.ActiveThemeId,
                        principalTable: "PortfolioThemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfileThemeSettings_PortfolioThemes_DraftThemeId",
                        column: x => x.DraftThemeId,
                        principalTable: "PortfolioThemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfileThemeSettings_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioThemes_Key",
                table: "PortfolioThemes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileThemeSettings_ActiveThemeId",
                table: "ProfileThemeSettings",
                column: "ActiveThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileThemeSettings_DraftThemeId",
                table: "ProfileThemeSettings",
                column: "DraftThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileThemeSettings_ProfileId",
                table: "ProfileThemeSettings",
                column: "ProfileId",
                unique: true);
        }
    }
}
