using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loan.Management.Migrations
{
    /// <inheritdoc />
    public partial class AddedSignerCertificates : Migration
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
                name: "SignerCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SignerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivateKeyData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Thumbprint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Issuer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_SignerCertificates", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CertifierProfiles_Persons_HolderProfileId",
                table: "CertifierProfiles",
                column: "HolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_IssuerProfiles_Persons_HolderProfileId",
                table: "IssuerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_VerifierProfiles_Persons_HolderProfileId",
                table: "VerifierProfiles");

            migrationBuilder.DropTable(
                name: "SignerCertificates");

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
