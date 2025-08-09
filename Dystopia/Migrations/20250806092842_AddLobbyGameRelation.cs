using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class AddLobbyGameRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LobbyId",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Games_LobbyId",
                table: "Games",
                column: "LobbyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Lobbies_LobbyId",
                table: "Games",
                column: "LobbyId",
                principalTable: "Lobbies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Lobbies_LobbyId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_LobbyId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "LobbyId",
                table: "Games");
        }
    }
}
