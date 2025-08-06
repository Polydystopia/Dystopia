using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class AddLobbyState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartedGameId",
                table: "Lobbies");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Lobbies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Lobbies");

            migrationBuilder.AddColumn<Guid>(
                name: "StartedGameId",
                table: "Lobbies",
                type: "TEXT",
                nullable: true);
        }
    }
}
