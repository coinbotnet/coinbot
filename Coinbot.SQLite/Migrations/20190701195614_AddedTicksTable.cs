using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coinbot.SQLite.Migrations
{
    public partial class AddedTicksTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ticks",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Stock = table.Column<string>(nullable: true),
                    Ask = table.Column<double>(nullable: false),
                    Bid = table.Column<double>(nullable: false),
                    Last = table.Column<double>(nullable: false),
                    BaseCoin = table.Column<string>(nullable: true),
                    TargetCoin = table.Column<string>(nullable: true),
                    InsertedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ticks_BaseCoin_TargetCoin_Stock",
                table: "Ticks",
                columns: new[] { "BaseCoin", "TargetCoin", "Stock" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ticks");
        }
    }
}
