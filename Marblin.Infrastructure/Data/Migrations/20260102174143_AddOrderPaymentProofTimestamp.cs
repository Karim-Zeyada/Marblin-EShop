using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marblin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentProofTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentProofSubmittedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentProofSubmittedAt",
                table: "Orders");
        }
    }
}
