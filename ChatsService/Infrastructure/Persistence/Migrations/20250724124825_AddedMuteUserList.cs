using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatsService.Migrations
{
    /// <inheritdoc />
    public partial class AddedMuteUserList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserSnapshotId",
                schema: "chats",
                table: "UserSnapshots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSnapshots_UserSnapshotId",
                schema: "chats",
                table: "UserSnapshots",
                column: "UserSnapshotId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSnapshots_UserSnapshots_UserSnapshotId",
                schema: "chats",
                table: "UserSnapshots",
                column: "UserSnapshotId",
                principalSchema: "chats",
                principalTable: "UserSnapshots",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSnapshots_UserSnapshots_UserSnapshotId",
                schema: "chats",
                table: "UserSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_UserSnapshots_UserSnapshotId",
                schema: "chats",
                table: "UserSnapshots");

            migrationBuilder.DropColumn(
                name: "UserSnapshotId",
                schema: "chats",
                table: "UserSnapshots");
        }
    }
}
