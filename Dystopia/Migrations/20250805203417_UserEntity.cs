using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class UserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alias",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CmUserData",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Defeats",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FriendCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MultiplayerRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NumFriends",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NumGames",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NumMultiplayergames",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UnlockedSkins",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UnlockedTribes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserMigrated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Victories",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Participators",
                table: "Lobbies");

            migrationBuilder.RenameColumn(
                name: "PolytopiaId",
                table: "Users",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SteamId",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginDate",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GameVersions",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "AvatarStateData",
                table: "Users",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "TEXT",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserEntityId",
                table: "Friends",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameParticipators",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DateLastCommand = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateLastStartTurn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateLastEndTurn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateCurrentTurnDeadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeBank = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    LastConsumedTimeBank = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    InvitationState = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedTribe = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedTribeSkin = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameParticipators", x => new { x.GameId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GameParticipators_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameParticipators_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LobbyParticipators",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LobbyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitationState = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedTribe = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedTribeSkin = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LobbyParticipators", x => new { x.LobbyId, x.UserId });
                    table.ForeignKey(
                        name: "FK_LobbyParticipators_Lobbies_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "Lobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LobbyParticipators_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserEntityId",
                table: "Friends",
                column: "UserEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GameParticipators_UserId",
                table: "GameParticipators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyParticipators_UserId",
                table: "LobbyParticipators",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_Users_UserEntityId",
                table: "Friends",
                column: "UserEntityId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_Users_UserEntityId",
                table: "Friends");

            migrationBuilder.DropTable(
                name: "GameParticipators");

            migrationBuilder.DropTable(
                name: "LobbyParticipators");

            migrationBuilder.DropIndex(
                name: "IX_Friends_UserEntityId",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserEntityId",
                table: "Friends");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "PolytopiaId");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "SteamId",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginDate",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "GameVersions",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<byte[]>(
                name: "AvatarStateData",
                table: "Users",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CmUserData",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Defeats",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FriendCode",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MultiplayerRating",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumFriends",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumGames",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumMultiplayergames",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnlockedSkins",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnlockedTribes",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UserMigrated",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Victories",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Participators",
                table: "Lobbies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
