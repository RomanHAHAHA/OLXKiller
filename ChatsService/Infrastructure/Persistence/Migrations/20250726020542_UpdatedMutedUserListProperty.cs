using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatsService.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedMutedUserListProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "UserMutes",
                schema: "chats",
                columns: table => new
                {
                    MutingUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MutedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMutes", x => new { x.MutingUserId, x.MutedUserId });
                    table.ForeignKey(
                        name: "FK_UserMutes_UserSnapshots_MutedUserId",
                        column: x => x.MutedUserId,
                        principalSchema: "chats",
                        principalTable: "UserSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMutes_UserSnapshots_MutingUserId",
                        column: x => x.MutingUserId,
                        principalSchema: "chats",
                        principalTable: "UserSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMutes_MutedUserId",
                schema: "chats",
                table: "UserMutes",
                column: "MutedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMutes",
                schema: "chats");

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
    }
}
