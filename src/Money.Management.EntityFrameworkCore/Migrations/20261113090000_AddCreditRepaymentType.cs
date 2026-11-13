using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Money.Management.EntityFrameworkCore;

#nullable disable

namespace Money.Management.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MoneyManagementDbContext))]
    [Migration("20261113090000_AddCreditRepaymentType")]
    public partial class AddCreditRepaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing loans default to long-term (0), the same as before this column existed.
            migrationBuilder.AddColumn<int>(
                name: "RepaymentType",
                table: "Credits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepaymentType",
                table: "Credits");
        }
    }
}
