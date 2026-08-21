using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Management.Migrations
{
    /// <inheritdoc />
    public partial class TrimBorrowerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Borrowers_IsKycVerified",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "AlternativePhoneNumber",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "BlacklistReason",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "CreditRating",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "CreditScore",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationship",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "Employer",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "EmploymentStartDate",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "IdExpiryDate",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "IsKycVerified",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "KycVerificationDate",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "LastActivityDate",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "NumberOfDependents",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Borrowers");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "Borrowers",
                newName: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Borrowers",
                newName: "StreetAddress");

            migrationBuilder.AddColumn<string>(
                name: "AlternativePhoneNumber",
                table: "Borrowers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Borrowers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlacklistReason",
                table: "Borrowers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Borrowers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Borrowers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreditRating",
                table: "Borrowers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreditScore",
                table: "Borrowers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Borrowers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Borrowers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Borrowers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelationship",
                table: "Borrowers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Employer",
                table: "Borrowers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EmploymentStartDate",
                table: "Borrowers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "EmploymentStatus",
                table: "Borrowers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "IdExpiryDate",
                table: "Borrowers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsKycVerified",
                table: "Borrowers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Borrowers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "KycVerificationDate",
                table: "Borrowers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityDate",
                table: "Borrowers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaritalStatus",
                table: "Borrowers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfDependents",
                table: "Borrowers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Borrowers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Borrowers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Borrowers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Borrowers_IsKycVerified",
                table: "Borrowers",
                column: "IsKycVerified");
        }
    }
}
