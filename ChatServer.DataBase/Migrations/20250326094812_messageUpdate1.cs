using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class messageUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastReadGroupMessageCount",
                table: "Users",
                newName: "LastReadGroupMessage");

            migrationBuilder.RenameColumn(
                name: "LastDeleteGroupMessageCount",
                table: "Users",
                newName: "LastDeleteGroupMessage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastReadGroupMessage",
                table: "Users",
                newName: "LastReadGroupMessageCount");

            migrationBuilder.RenameColumn(
                name: "LastDeleteGroupMessage",
                table: "Users",
                newName: "LastDeleteGroupMessageCount");
        }
    }
}
