using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class messageUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastDeleteFriendMessage",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastDeleteGroupMessageCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastReadFriendMessage",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastReadGroupMessageCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomHead",
                table: "Groups",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDeleteFriendMessage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastDeleteGroupMessageCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastReadFriendMessage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastReadGroupMessageCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsCustomHead",
                table: "Groups");
        }
    }
}
