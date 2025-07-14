using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainChecker.Migrations
{
    /// <inheritdoc />
    public partial class BackfillUserPreferencesIdForUserTelegramInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""UserTelegramInfo""
                SET ""UserPreferencesId"" = (
                    SELECT UP.""Id""
                    FROM ""UserPreferences"" AS UP
                    WHERE UP.""UserId"" = ""UserTelegramInfo"".""UserId""
                )
                WHERE ""UserPreferencesId"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""UserTelegramInfo""
                SET ""UserPreferencesId"" = NULL;
            ");
        }
    }
}
