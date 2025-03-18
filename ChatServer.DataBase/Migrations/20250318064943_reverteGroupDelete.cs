using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class reverteGroupDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GroupDeletes_GroupId",
                table: "GroupDeletes",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupDeletes_Groups_GroupId",
                table: "GroupDeletes",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupDeletes_Groups_GroupId",
                table: "GroupDeletes");

            migrationBuilder.DropIndex(
                name: "IX_GroupDeletes_GroupId",
                table: "GroupDeletes");
        }
    }
}
