using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loan.Management.Migrations
{
    /// <inheritdoc />
    public partial class fixed_value_objs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilitySlot");

            migrationBuilder.DropTable(
                name: "Specialization");

            migrationBuilder.AddColumn<string>(
                name: "AvailableSlots",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CapturedAt",
                table: "Persons",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

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

            migrationBuilder.AddColumn<string>(
                name: "FaceRecognitionHash",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FingerprintHash",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSlots",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "CapturedAt",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Cities",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "FaceRecognitionHash",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "FingerprintHash",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Longitude",
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
                name: "Value",
                table: "Persons");

            migrationBuilder.CreateTable(
                name: "AvailabilitySlot",
                columns: table => new
                {
                    CertifierAvailabilityCertifierProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilitySlot", x => new { x.CertifierAvailabilityCertifierProfileId, x.Id });
                    table.ForeignKey(
                        name: "FK_AvailabilitySlot_Persons_CertifierAvailabilityCertifierProfileId",
                        column: x => x.CertifierAvailabilityCertifierProfileId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Specialization",
                columns: table => new
                {
                    CertifierProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialization", x => new { x.CertifierProfileId, x.Id });
                    table.ForeignKey(
                        name: "FK_Specialization_Persons_CertifierProfileId",
                        column: x => x.CertifierProfileId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
