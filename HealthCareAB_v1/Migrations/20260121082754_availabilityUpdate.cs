using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthCareAB_v1.Migrations
{
    /// <inheritdoc />
    public partial class availabilityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_Users_CaregiverId",
                table: "Availabilities");

            migrationBuilder.DropIndex(
                name: "IX_Availabilities_CaregiverId",
                table: "Availabilities");

            migrationBuilder.AddColumn<Guid>(
                name: "MeetingId",
                table: "Feedbacks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeetingId",
                table: "Feedbacks");

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_CaregiverId",
                table: "Availabilities",
                column: "CaregiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_Users_CaregiverId",
                table: "Availabilities",
                column: "CaregiverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
