using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolytopiaB2.Carrier.Migrations
{
    /// <inheritdoc />
    public partial class AddLobbyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lobbies");
        }
    }
}
