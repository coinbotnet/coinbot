using Microsoft.EntityFrameworkCore.Migrations;

namespace Coinbot.SQLite.Migrations
{
    public partial class AddedIndexesSessionTicksTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ticks_BaseCoin_TargetCoin_Stock",
                table: "Ticks");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SellId_BuyId_Stock",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Ticks_BaseCoin_TargetCoin_Stock_SessionId",
                table: "Ticks",
                columns: new[] { "BaseCoin", "TargetCoin", "Stock", "SessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellId_BuyId_Stock_SessionId",
                table: "Orders",
                columns: new[] { "SellId", "BuyId", "Stock", "SessionId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ticks_BaseCoin_TargetCoin_Stock_SessionId",
                table: "Ticks");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SellId_BuyId_Stock_SessionId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Ticks_BaseCoin_TargetCoin_Stock",
                table: "Ticks",
                columns: new[] { "BaseCoin", "TargetCoin", "Stock" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellId_BuyId_Stock",
                table: "Orders",
                columns: new[] { "SellId", "BuyId", "Stock" });
        }
    }
}
