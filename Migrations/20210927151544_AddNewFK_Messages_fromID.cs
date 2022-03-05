using Microsoft.EntityFrameworkCore.Migrations;

namespace TinderClone.Migrations
{
    public partial class AddNewFK_Messages_fromID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Messages_fromID",
                table: "Messages",
                column: "fromID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_fromID",
                table: "Messages",
                column: "fromID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_fromID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_fromID",
                table: "Messages");
        }
    }
}
