using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Management.Migrations
{
    /// <inheritdoc />
    public partial class MakeBorrowerIdNumberNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Borrowers_IdNumber",
                table: "Borrowers");

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "Borrowers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);

            // Existing borrowers saved without an ID have "" — convert them to NULL
            migrationBuilder.Sql("UPDATE [Borrowers] SET [IdNumber] = NULL WHERE LTRIM(RTRIM([IdNumber])) = ''");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowers_IdNumber",
                table: "Borrowers",
                column: "IdNumber",
                unique: true,
                filter: "[IdNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Borrowers_IdNumber",
                table: "Borrowers");

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "Borrowers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Borrowers_IdNumber",
                table: "Borrowers",
                column: "IdNumber",
                unique: true);
        }
    }
}
