using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marblin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveArtisanPhilosophies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessSteps",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ValueStatements",
                table: "SiteSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessSteps",
                table: "SiteSettings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueStatements",
                table: "SiteSettings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
