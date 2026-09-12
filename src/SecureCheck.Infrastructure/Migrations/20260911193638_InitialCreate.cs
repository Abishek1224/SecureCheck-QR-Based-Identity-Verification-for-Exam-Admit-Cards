using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureCheck.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    RollNumber = table.Column<string>(type: "TEXT", nullable: false),
                    PhotoUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    ExamCentre = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VerificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvigilatorDeviceId = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ScannedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExamName = table.Column<string>(type: "TEXT", nullable: false),
                    ExamDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SeatNumber = table.Column<string>(type: "TEXT", nullable: false),
                    QrCodeToken = table.Column<string>(type: "TEXT", nullable: false),
                    AdmitCardPdfPath = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_QrCodeToken",
                table: "Registrations",
                column: "QrCodeToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_StudentId",
                table: "Registrations",
                column: "StudentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropTable(
                name: "VerificationLogs");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
