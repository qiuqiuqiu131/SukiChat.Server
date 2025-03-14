using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class unreadMessageCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastChatId",
                table: "GroupRelations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastChatId",
                table: "FriendRelations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastChatId",
                table: "GroupRelations");

            migrationBuilder.DropColumn(
                name: "LastChatId",
                table: "FriendRelations");
        }
    }
}
