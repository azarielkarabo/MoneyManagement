using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Management.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLoanStatusAndAddBudgetActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LoanStatus",
                table: "Loans",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_LoanStatus",
                table: "Loans",
                newName: "IX_Loans_Status");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualAmount",
                table: "BudgetItems",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualAmount",
                table: "BudgetItems");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Loans",
                newName: "LoanStatus");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_Status",
                table: "Loans",
                newName: "IX_Loans_LoanStatus");
        }
    }
}
