using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class messageUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDeleteFriendMessage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastDeleteGroupMessage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastReadFriendMessage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastReadGroupMessage",
                table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDeleteFriendMessageTime",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDeleteGroupMessageTime",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadFriendMessageTime",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadGroupMessageTime",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDeleteFriendMessageTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastDeleteGroupMessageTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastReadFriendMessageTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastReadGroupMessageTime",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "LastDeleteFriendMessage",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastDeleteGroupMessage",
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
                name: "LastReadGroupMessage",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
