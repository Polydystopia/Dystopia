using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class MaxPlayersLobby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "MaxPlayers",
                table: "Lobbies",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPlayers",
                table: "Lobbies");
        }
    }
}
