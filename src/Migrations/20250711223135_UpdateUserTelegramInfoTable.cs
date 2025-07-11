using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainChecker.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTelegramInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedPassword",
                table: "UserTelegramInfo");

            migrationBuilder.DropColumn(
                name: "TelegramUsername",
                table: "UserTelegramInfo");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedBotToken",
                table: "UserTelegramInfo",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedBotToken",
                table: "UserTelegramInfo");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedPassword",
                table: "UserTelegramInfo",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelegramUsername",
                table: "UserTelegramInfo",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
