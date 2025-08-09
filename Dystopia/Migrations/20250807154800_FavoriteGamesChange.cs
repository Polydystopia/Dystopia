using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class FavoriteGamesChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteGames_Games_GameId",
                table: "UserFavoriteGames");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteGames_Users_UserId",
                table: "UserFavoriteGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteGames",
                table: "UserFavoriteGames");

            migrationBuilder.DropIndex(
                name: "IX_UserFavoriteGames_GameId",
                table: "UserFavoriteGames");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserFavoriteGames");

            migrationBuilder.RenameColumn(
                name: "MarkedAt",
                table: "UserFavoriteGames",
                newName: "UserEntityId");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "UserFavoriteGames",
                newName: "FavoriteGamesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteGames",
                table: "UserFavoriteGames",
                columns: new[] { "FavoriteGamesId", "UserEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteGames_UserEntityId",
                table: "UserFavoriteGames",
                column: "UserEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteGames_Games_FavoriteGamesId",
                table: "UserFavoriteGames",
                column: "FavoriteGamesId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteGames_Users_UserEntityId",
                table: "UserFavoriteGames",
                column: "UserEntityId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteGames_Games_FavoriteGamesId",
                table: "UserFavoriteGames");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteGames_Users_UserEntityId",
                table: "UserFavoriteGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteGames",
                table: "UserFavoriteGames");

            migrationBuilder.DropIndex(
                name: "IX_UserFavoriteGames_UserEntityId",
                table: "UserFavoriteGames");

            migrationBuilder.RenameColumn(
                name: "UserEntityId",
                table: "UserFavoriteGames",
                newName: "MarkedAt");

            migrationBuilder.RenameColumn(
                name: "FavoriteGamesId",
                table: "UserFavoriteGames",
                newName: "GameId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "UserFavoriteGames",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteGames",
                table: "UserFavoriteGames",
                columns: new[] { "UserId", "GameId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteGames_GameId",
                table: "UserFavoriteGames",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteGames_Games_GameId",
                table: "UserFavoriteGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteGames_Users_UserId",
                table: "UserFavoriteGames",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
