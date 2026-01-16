using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marblin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBalancePaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BalancePaymentProofSubmittedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BalancePaymentProofType",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BalanceReceiptImageUrl",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BalanceTransactionId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BalanceVerifiedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBalanceVerified",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalancePaymentProofSubmittedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BalancePaymentProofType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BalanceReceiptImageUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BalanceTransactionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BalanceVerifiedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsBalanceVerified",
                table: "Orders");
        }
    }
}
