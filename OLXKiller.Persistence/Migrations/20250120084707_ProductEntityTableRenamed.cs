using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OLXKiller.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductEntityTableRenamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntity_Users_SellerId",
                table: "ProductEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductEntity",
                table: "ProductEntity");

            migrationBuilder.RenameTable(
                name: "ProductEntity",
                newName: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntity_SellerId",
                table: "Products",
                newName: "IX_Products_SellerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "ProductEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Products_SellerId",
                table: "ProductEntity",
                newName: "IX_ProductEntity_SellerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductEntity",
                table: "ProductEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntity_Users_SellerId",
                table: "ProductEntity",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
