using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coinbot.SQLite.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    SellId = table.Column<string>(nullable: true),
                    BuyId = table.Column<string>(nullable: true),
                    BaseCoin = table.Column<string>(nullable: true),
                    TargetCoin = table.Column<string>(nullable: true),
                    ChangeToSell = table.Column<double>(nullable: false),
                    Stack = table.Column<double>(nullable: false),
                    BoughtFor = table.Column<double>(nullable: false),
                    ToSellFor = table.Column<double>(nullable: false),
                    SoldFor = table.Column<double>(nullable: true),
                    QuantityBought = table.Column<double>(nullable: false),
                    QuantitySold = table.Column<double>(nullable: false),
                    SellOrderPlaced = table.Column<bool>(nullable: false),
                    Sold = table.Column<bool>(nullable: false),
                    Bought = table.Column<bool>(nullable: false),
                    SessionId = table.Column<string>(nullable: true),
                    InsertedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    Stock = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
