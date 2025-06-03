using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolytopiaB2.Carrier.Migrations
{
    /// <inheritdoc />
    public partial class AddGameViewModelTable : Migration
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
                    GameSettingsJson = table.Column<string>(type: "TEXT", nullable: true),
                    InitialGameStateData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CurrentGameStateData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    TimerSettings = table.Column<string>(type: "TEXT", nullable: true),
                    DateCurrentTurnDeadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GameContext = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
