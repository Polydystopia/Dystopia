using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class DeleteLobbyMatchmakingCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matchmaking_Lobbies_LobbyGameViewModelId",
                table: "Matchmaking");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchmaking_Lobbies_LobbyGameViewModelId",
                table: "Matchmaking",
                column: "LobbyGameViewModelId",
                principalTable: "Lobbies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matchmaking_Lobbies_LobbyGameViewModelId",
                table: "Matchmaking");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchmaking_Lobbies_LobbyGameViewModelId",
                table: "Matchmaking",
                column: "LobbyGameViewModelId",
                principalTable: "Lobbies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
