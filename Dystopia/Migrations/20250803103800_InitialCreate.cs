using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateLastCommand = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    GameSettings = table.Column<string>(type: "TEXT", nullable: false),
                    InitialGameStateData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CurrentGameStateData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    TimerSettings = table.Column<string>(type: "TEXT", nullable: false),
                    DateCurrentTurnDeadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExternalTournamentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ExternalMatchId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lobbies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedReason = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    MapPreset = table.Column<int>(type: "INTEGER", nullable: false),
                    MapSize = table.Column<int>(type: "INTEGER", nullable: false),
                    OpponentCount = table.Column<short>(type: "INTEGER", nullable: false),
                    GameMode = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisabledTribes = table.Column<string>(type: "TEXT", nullable: true),
                    StartedGameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsPersistent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSharable = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    ScoreLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    InviteLink = table.Column<string>(type: "TEXT", nullable: true),
                    MatchmakingGameId = table.Column<long>(type: "INTEGER", nullable: true),
                    ChallengermodeGameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GameContext = table.Column<string>(type: "TEXT", nullable: true),
                    Participators = table.Column<string>(type: "TEXT", nullable: true),
                    Bots = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lobbies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NewsType = table.Column<int>(type: "INTEGER", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    Image = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    PolytopiaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Alias = table.Column<string>(type: "TEXT", nullable: true),
                    FriendCode = table.Column<string>(type: "TEXT", nullable: true),
                    AllowsFriendRequests = table.Column<bool>(type: "INTEGER", nullable: false),
                    SteamId = table.Column<string>(type: "TEXT", nullable: true),
                    NumFriends = table.Column<int>(type: "INTEGER", nullable: true),
                    Elo = table.Column<int>(type: "INTEGER", nullable: false),
                    Victories = table.Column<string>(type: "TEXT", nullable: true),
                    Defeats = table.Column<string>(type: "TEXT", nullable: true),
                    NumGames = table.Column<int>(type: "INTEGER", nullable: true),
                    NumMultiplayergames = table.Column<int>(type: "INTEGER", nullable: true),
                    MultiplayerRating = table.Column<int>(type: "INTEGER", nullable: true),
                    AvatarStateData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    UserMigrated = table.Column<bool>(type: "INTEGER", nullable: false),
                    GameVersions = table.Column<string>(type: "TEXT", nullable: true),
                    LastLoginDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UnlockedTribes = table.Column<string>(type: "TEXT", nullable: true),
                    UnlockedSkins = table.Column<string>(type: "TEXT", nullable: true),
                    CmUserData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.PolytopiaId);
                });

            migrationBuilder.CreateTable(
                name: "Matchmaking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LobbyGameViewModelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    MapSize = table.Column<int>(type: "INTEGER", nullable: false),
                    MapPreset = table.Column<int>(type: "INTEGER", nullable: false),
                    GameMode = table.Column<int>(type: "INTEGER", nullable: false),
                    ScoreLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowCrossPlay = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxPlayers = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerIds = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchmaking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matchmaking_Lobbies_LobbyGameViewModelId",
                        column: x => x.LobbyGameViewModelId,
                        principalTable: "Lobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    UserId1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId2 = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => new { x.UserId1, x.UserId2 });
                    table.ForeignKey(
                        name: "FK_Friends_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "PolytopiaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Friends_Users_UserId2",
                        column: x => x.UserId2,
                        principalTable: "Users",
                        principalColumn: "PolytopiaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoriteGames",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MarkedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteGames", x => new { x.UserId, x.GameId });
                    table.ForeignKey(
                        name: "FK_UserFavoriteGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteGames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "PolytopiaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId2",
                table: "Friends",
                column: "UserId2");

            migrationBuilder.CreateIndex(
                name: "IX_Matchmaking_LobbyGameViewModelId",
                table: "Matchmaking",
                column: "LobbyGameViewModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteGames_GameId",
                table: "UserFavoriteGames",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "Matchmaking");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "UserFavoriteGames");

            migrationBuilder.DropTable(
                name: "Lobbies");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
