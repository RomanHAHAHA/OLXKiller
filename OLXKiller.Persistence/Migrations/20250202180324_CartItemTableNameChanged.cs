using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OLXKiller.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CartItemTableNameChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItemEntity_Products_ProductId",
                table: "CartItemEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItemEntity_Users_UserId",
                table: "CartItemEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItemEntity",
                table: "CartItemEntity");

            migrationBuilder.RenameTable(
                name: "CartItemEntity",
                newName: "CartItems");

            migrationBuilder.RenameIndex(
                name: "IX_CartItemEntity_ProductId",
                table: "CartItems",
                newName: "IX_CartItems_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Users_UserId",
                table: "CartItems",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Users_UserId",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems");

            migrationBuilder.RenameTable(
                name: "CartItems",
                newName: "CartItemEntity");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItemEntity",
                newName: "IX_CartItemEntity_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItemEntity",
                table: "CartItemEntity",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CartItemEntity_Products_ProductId",
                table: "CartItemEntity",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItemEntity_Users_UserId",
                table: "CartItemEntity",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
