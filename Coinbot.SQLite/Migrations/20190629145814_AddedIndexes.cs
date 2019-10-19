using Microsoft.EntityFrameworkCore.Migrations;

namespace Coinbot.SQLite.Migrations
{
    public partial class AddedIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellId_BuyId_Stock",
                table: "Orders",
                columns: new[] { "SellId", "BuyId", "Stock" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_SellId_BuyId_Stock",
                table: "Orders");
        }
    }
}
