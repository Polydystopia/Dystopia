using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolytopiaB2.Carrier.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    PolytopiaId = table.Column<Guid>(type: "TEXT", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId2",
                table: "Friends",
                column: "UserId2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
