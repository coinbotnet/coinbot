using Microsoft.EntityFrameworkCore.Migrations;

namespace Coinbot.SQLite.Migrations
{
    public partial class AddedSessionTicksTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Ticks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Ticks");
        }
    }
}
