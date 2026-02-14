using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marblin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantSalePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SalePrice",
                table: "ProductVariants",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalePrice",
                table: "ProductVariants");
        }
    }
}
