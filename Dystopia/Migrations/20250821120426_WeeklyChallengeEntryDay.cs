using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dystopia.Migrations
{
    /// <inheritdoc />
    public partial class WeeklyChallengeEntryDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "WeeklyChallengeEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentLeagueId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "WeeklyChallengeEntries");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentLeagueId",
                table: "Users",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
