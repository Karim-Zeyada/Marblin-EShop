using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marblin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementsAndSalePricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeaturedSale",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaleEndDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalePrice",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaleStartDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Style = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ShowOnHomepage = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnCatalog = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnCheckout = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeaturedSale",
                table: "Products",
                column: "IsFeaturedSale");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_EndDate",
                table: "Announcements",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_IsActive",
                table: "Announcements",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_StartDate",
                table: "Announcements",
                column: "StartDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsFeaturedSale",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFeaturedSale",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaleEndDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalePrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaleStartDate",
                table: "Products");
        }
    }
}
