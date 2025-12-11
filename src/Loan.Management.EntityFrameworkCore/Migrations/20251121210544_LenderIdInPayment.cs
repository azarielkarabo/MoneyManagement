using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loan.Management.Migrations
{
    /// <inheritdoc />
    public partial class LenderIdInPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LenderId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LenderId",
                table: "Payments",
                column: "LenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Lenders_LenderId",
                table: "Payments",
                column: "LenderId",
                principalTable: "Lenders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Lenders_LenderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_LenderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LenderId",
                table: "Payments");
        }
    }
}
