using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolytopiaB2.Carrier.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchmakingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sender",
                table: "Friends");

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
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matchmaking_LobbyGameViewModelId",
                table: "Matchmaking",
                column: "LobbyGameViewModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matchmaking");

            migrationBuilder.AddColumn<int>(
                name: "Sender",
                table: "Friends",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
