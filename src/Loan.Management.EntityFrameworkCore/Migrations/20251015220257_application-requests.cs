using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loan.Management.Migrations
{
    /// <inheritdoc />
    public partial class applicationrequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CertifierProfiles_Persons_HolderProfileId",
                table: "CertifierProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_IssuerProfiles_Persons_HolderProfileId",
                table: "IssuerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_VerifierProfiles_Persons_HolderProfileId",
                table: "VerifierProfiles");

            migrationBuilder.AlterColumn<Guid>(
                name: "HolderProfileId",
                table: "VerifierProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "HolderProfileId",
                table: "IssuerProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationRequestId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "HolderProfileId",
                table: "CertifierProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    HolderProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertifierType = table.Column<int>(type: "int", nullable: true),
                    Justification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationRequests_Persons_HolderProfileId",
                        column: x => x.HolderProfileId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApplicationRequestId",
                table: "Documents",
                column: "ApplicationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRequests_HolderProfileId",
                table: "ApplicationRequests",
                column: "HolderProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_CertifierProfiles_Persons_HolderProfileId",
                table: "CertifierProfiles",
                column: "HolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_ApplicationRequests_ApplicationRequestId",
                table: "Documents",
                column: "ApplicationRequestId",
                principalTable: "ApplicationRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IssuerProfiles_Persons_HolderProfileId",
                table: "IssuerProfiles",
                column: "HolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VerifierProfiles_Persons_HolderProfileId",
                table: "VerifierProfiles",
                column: "HolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CertifierProfiles_Persons_HolderProfileId",
                table: "CertifierProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_ApplicationRequests_ApplicationRequestId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_IssuerProfiles_Persons_HolderProfileId",
                table: "IssuerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_VerifierProfiles_Persons_HolderProfileId",
                table: "VerifierProfiles");

            migrationBuilder.DropTable(
                name: "ApplicationRequests");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ApplicationRequestId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ApplicationRequestId",
                table: "Documents");

            migrationBuilder.AlterColumn<Guid>(
                name: "HolderProfileId",
                table: "VerifierProfiles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "HolderProfileId",
                table: "IssuerProfiles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "HolderProfileId",
                table: "CertifierProfiles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_CertifierProfiles_Persons_HolderProfileId",
                table: "CertifierProfiles",
                column: "HolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IssuerProfiles_Persons_HolderProfileId",
                table: "IssuerProfiles",
                column: "HolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VerifierProfiles_Persons_HolderProfileId",
                table: "VerifierProfiles",
                column: "HolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id");
        }
    }
}
