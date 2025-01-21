using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OLXKiller.Persistence.Migrations;

/// <inheritdoc />
public partial class ProductEntityCreated : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProductEntity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Amount = table.Column<int>(type: "int", nullable: false),
                SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductEntity", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductEntity_Users_SellerId",
                    column: x => x.SellerId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProductEntity_SellerId",
            table: "ProductEntity",
            column: "SellerId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProductEntity");
    }
}
