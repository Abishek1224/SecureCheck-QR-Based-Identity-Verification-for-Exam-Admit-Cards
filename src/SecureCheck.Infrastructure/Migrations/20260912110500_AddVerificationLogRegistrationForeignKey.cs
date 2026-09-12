using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureCheck.Infrastructure.Migrations
{
    public partial class AddVerificationLogRegistrationForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VerificationLogs_RegistrationId",
                table: "VerificationLogs",
                column: "RegistrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationLogs_Registrations_RegistrationId",
                table: "VerificationLogs",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VerificationLogs_Registrations_RegistrationId",
                table: "VerificationLogs");

            migrationBuilder.DropIndex(
                name: "IX_VerificationLogs_RegistrationId",
                table: "VerificationLogs");
        }
    }
}
