using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loan.Management.Migrations
{
    /// <inheritdoc />
    public partial class Fix_CertifierId_FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentCertificationRequests_CertifierProfiles_CertifierProfileId",
                table: "DocumentCertificationRequests");

            migrationBuilder.DropIndex(
                name: "IX_DocumentCertificationRequests_CertifierProfileId",
                table: "DocumentCertificationRequests");

            migrationBuilder.DropColumn(
                name: "CertifierProfileId",
                table: "DocumentCertificationRequests");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCertificationRequests_CertifierId",
                table: "DocumentCertificationRequests",
                column: "CertifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentCertificationRequests_CertifierProfiles_CertifierId",
                table: "DocumentCertificationRequests",
                column: "CertifierId",
                principalTable: "CertifierProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentCertificationRequests_CertifierProfiles_CertifierId",
                table: "DocumentCertificationRequests");

            migrationBuilder.DropIndex(
                name: "IX_DocumentCertificationRequests_CertifierId",
                table: "DocumentCertificationRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "CertifierProfileId",
                table: "DocumentCertificationRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCertificationRequests_CertifierProfileId",
                table: "DocumentCertificationRequests",
                column: "CertifierProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentCertificationRequests_CertifierProfiles_CertifierProfileId",
                table: "DocumentCertificationRequests",
                column: "CertifierProfileId",
                principalTable: "CertifierProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
