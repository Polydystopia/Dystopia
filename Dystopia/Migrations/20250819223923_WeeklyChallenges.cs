using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class WeeklyChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentLeagueId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LocalizationKey = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryColor = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondaryColor = table.Column<int>(type: "INTEGER", nullable: false),
                    TertiaryColor = table.Column<int>(type: "INTEGER", nullable: false),
                    PromotionRate = table.Column<float>(type: "REAL", nullable: false),
                    DemotionRate = table.Column<float>(type: "REAL", nullable: false),
                    IsFriendsLeague = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEntry = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Week = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Tribe = table.Column<int>(type: "INTEGER", nullable: false),
                    SkinType = table.Column<int>(type: "INTEGER", nullable: false),
                    GameVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscordLink = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyChallengeEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WeeklyChallengeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LeagueId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    HasFinished = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasReplay = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsValid = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyChallengeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyChallengeEntries_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklyChallengeEntries_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklyChallengeEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklyChallengeEntries_WeeklyChallenges_WeeklyChallengeId",
                        column: x => x.WeeklyChallengeId,
                        principalTable: "WeeklyChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentLeagueId",
                table: "Users",
                column: "CurrentLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyChallengeEntries_GameId",
                table: "WeeklyChallengeEntries",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyChallengeEntries_LeagueId",
                table: "WeeklyChallengeEntries",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyChallengeEntries_UserId",
                table: "WeeklyChallengeEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyChallengeEntries_WeeklyChallengeId",
                table: "WeeklyChallengeEntries",
                column: "WeeklyChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Leagues_CurrentLeagueId",
                table: "Users",
                column: "CurrentLeagueId",
                principalTable: "Leagues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Leagues_CurrentLeagueId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "WeeklyChallengeEntries");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "WeeklyChallenges");

            migrationBuilder.DropIndex(
                name: "IX_Users_CurrentLeagueId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrentLeagueId",
                table: "Users");
        }
    }
}
