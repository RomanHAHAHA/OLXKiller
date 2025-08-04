using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatsService.Migrations
{
    /// <inheritdoc />
    public partial class AddedOnlinePropertyToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                schema: "chats",
                table: "UserSnapshots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOnlineAt",
                schema: "chats",
                table: "UserSnapshots",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnline",
                schema: "chats",
                table: "UserSnapshots");

            migrationBuilder.DropColumn(
                name: "LastOnlineAt",
                schema: "chats",
                table: "UserSnapshots");
        }
    }
}
