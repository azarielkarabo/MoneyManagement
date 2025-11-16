using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loan.Management.Migrations
{
    /// <inheritdoc />
    public partial class Addedconfigsfordocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerificationCode",
                table: "VerificationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificationAuditTrail",
                table: "Documents",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CertificationCertifierId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CertificationCertifierType",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CertificationTimestamp",
                table: "Documents",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "CertificationType",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "ContentData",
                table: "Documents",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "Documents",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentMimeType",
                table: "Documents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ContentSize",
                table: "Documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MetadataContentType",
                table: "Documents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MetadataCreatedAt",
                table: "Documents",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "MetadataExtractedData",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "MetadataFileSizeBytes",
                table: "Documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MetadataOriginalFileName",
                table: "Documents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SignatureAlgorithm",
                table: "Documents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SignatureCertificate",
                table: "Documents",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SignatureSignedAt",
                table: "Documents",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "SignatureValue",
                table: "Documents",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VerificationCode",
                table: "Documents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationCode",
                table: "VerificationRequests");

            migrationBuilder.DropColumn(
                name: "CertificationAuditTrail",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CertificationCertifierId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CertificationCertifierType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CertificationTimestamp",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CertificationType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ContentData",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ContentMimeType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ContentSize",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "MetadataContentType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "MetadataCreatedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "MetadataExtractedData",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "MetadataFileSizeBytes",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "MetadataOriginalFileName",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "SignatureAlgorithm",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "SignatureCertificate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "SignatureSignedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "SignatureValue",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "VerificationCode",
                table: "Documents");
        }
    }
}
