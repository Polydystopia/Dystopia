using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyChallengeEntryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeeklyChallengeEntries_LeagueId",
                table: "WeeklyChallengeEntries");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyChallengeEntry_League_Challenge_Day_Score",
                table: "WeeklyChallengeEntries",
                columns: new[] { "LeagueId", "WeeklyChallengeId", "Day", "Score" });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyChallengeEntry_League_Challenge_Score",
                table: "WeeklyChallengeEntries",
                columns: new[] { "LeagueId", "WeeklyChallengeId", "Score" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeeklyChallengeEntry_League_Challenge_Day_Score",
                table: "WeeklyChallengeEntries");

            migrationBuilder.DropIndex(
                name: "IX_WeeklyChallengeEntry_League_Challenge_Score",
                table: "WeeklyChallengeEntries");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyChallengeEntries_LeagueId",
                table: "WeeklyChallengeEntries",
                column: "LeagueId");
        }
    }
}
