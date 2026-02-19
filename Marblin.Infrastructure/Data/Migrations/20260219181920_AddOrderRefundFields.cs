using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marblin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderRefundFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRefunded",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRefunded",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "Orders");
        }
    }
}
