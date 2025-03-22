using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class IsChatting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChatting",
                table: "GroupRelations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsChatting",
                table: "FriendRelations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChatting",
                table: "GroupRelations");

            migrationBuilder.DropColumn(
                name: "IsChatting",
                table: "FriendRelations");
        }
    }
}
