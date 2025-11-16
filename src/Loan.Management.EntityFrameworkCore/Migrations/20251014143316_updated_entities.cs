using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loan.Management.Migrations
{
    /// <inheritdoc />
    public partial class updated_entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalCredentials_Persons_CertifierProfileId",
                table: "ProfessionalCredentials");

            migrationBuilder.DropColumn(
                name: "AvailableSlots",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "CertificationStatus",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "CertificationsCompleted",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Cities",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "DocumentsIssuedThisMonth",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "MonthlyDocumentLimit",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Provinces",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "RadiusKm",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "VerificationsThisMonth",
                table: "Persons");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Persons",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);

            migrationBuilder.AddColumn<Guid>(
                name: "HolderProfileId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IssuerHolderProfileId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CertifierProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CertificationStatus = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    Longitude = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RadiusKm = table.Column<int>(type: "int", nullable: false),
                    Provinces = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableSlots = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HolderProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CertificationsCompleted = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_CertifierProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertifierProfiles_Persons_HolderProfileId",
                        column: x => x.HolderProfileId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IssuerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HolderProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MonthlyDocumentLimit = table.Column<int>(type: "int", nullable: false),
                    DocumentsIssuedThisMonth = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_IssuerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssuerProfiles_Persons_HolderProfileId",
                        column: x => x.HolderProfileId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VerifierProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VerificationsThisMonth = table.Column<int>(type: "int", nullable: false),
                    HolderProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_VerifierProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerifierProfiles_Persons_HolderProfileId",
                        column: x => x.HolderProfileId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_UserId",
                table: "Persons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_HolderProfileId",
                table: "Documents",
                column: "HolderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IssuerHolderProfileId",
                table: "Documents",
                column: "IssuerHolderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CertifierProfiles_HolderProfileId",
                table: "CertifierProfiles",
                column: "HolderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuerProfiles_HolderProfileId",
                table: "IssuerProfiles",
                column: "HolderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_VerifierProfiles_HolderProfileId",
                table: "VerifierProfiles",
                column: "HolderProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Persons_HolderProfileId",
                table: "Documents",
                column: "HolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Persons_IssuerHolderProfileId",
                table: "Documents",
                column: "IssuerHolderProfileId",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_AbpUsers_UserId",
                table: "Persons",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalCredentials_CertifierProfiles_CertifierProfileId",
                table: "ProfessionalCredentials",
                column: "CertifierProfileId",
                principalTable: "CertifierProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Persons_HolderProfileId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Persons_IssuerHolderProfileId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Persons_AbpUsers_UserId",
                table: "Persons");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalCredentials_CertifierProfiles_CertifierProfileId",
                table: "ProfessionalCredentials");

            migrationBuilder.DropTable(
                name: "CertifierProfiles");

            migrationBuilder.DropTable(
                name: "IssuerProfiles");

            migrationBuilder.DropTable(
                name: "VerifierProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Persons_UserId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Documents_HolderProfileId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_IssuerHolderProfileId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "HolderProfileId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IssuerHolderProfileId",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Persons",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<string>(
                name: "AvailableSlots",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Persons",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CertificationStatus",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CertificationsCompleted",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cities",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentsIssuedThisMonth",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Persons",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Persons",
                type: "float(18)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Persons",
                type: "float(18)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyDocumentLimit",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provinces",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RadiusKm",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Persons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationsThisMonth",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalCredentials_Persons_CertifierProfileId",
                table: "ProfessionalCredentials",
                column: "CertifierProfileId",
                principalTable: "Persons",
                principalColumn: "Id");
        }
    }
}
