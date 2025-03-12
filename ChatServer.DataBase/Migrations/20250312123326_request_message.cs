using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class request_message : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "GroupRequests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "FriendRequests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "GroupRequests");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "FriendRequests");
        }
    }
}
