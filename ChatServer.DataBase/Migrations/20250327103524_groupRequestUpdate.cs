using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class groupRequestUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Grouping",
                table: "GroupRequests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "GroupRequests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "GroupRequests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grouping",
                table: "GroupRequests");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "GroupRequests");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "GroupRequests");
        }
    }
}
