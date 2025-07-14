using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainChecker.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTelegramInfoToUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserTelegramInfoId",
                table: "UserPreferences",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserTelegramInfoId",
                table: "UserPreferences",
                column: "UserTelegramInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_UserTelegramInfo_UserTelegramInfoId",
                table: "UserPreferences",
                column: "UserTelegramInfoId",
                principalTable: "UserTelegramInfo",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_UserTelegramInfo_UserTelegramInfoId",
                table: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferences_UserTelegramInfoId",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UserTelegramInfoId",
                table: "UserPreferences");
        }
    }
}
