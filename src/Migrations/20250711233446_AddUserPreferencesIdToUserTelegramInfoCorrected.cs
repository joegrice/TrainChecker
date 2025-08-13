using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainChecker.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferencesIdToUserTelegramInfoCorrected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "UserPreferencesId",
                table: "UserTelegramInfo",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTelegramInfo_UserPreferencesId",
                table: "UserTelegramInfo",
                column: "UserPreferencesId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTelegramInfo_UserPreferences_UserPreferencesId",
                table: "UserTelegramInfo",
                column: "UserPreferencesId",
                principalTable: "UserPreferences",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTelegramInfo_UserPreferences_UserPreferencesId",
                table: "UserTelegramInfo");

            migrationBuilder.DropIndex(
                name: "IX_UserTelegramInfo_UserPreferencesId",
                table: "UserTelegramInfo");

            migrationBuilder.DropColumn(
                name: "UserPreferencesId",
                table: "UserTelegramInfo");

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
    }
}
