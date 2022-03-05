using Microsoft.EntityFrameworkCore.Migrations;

namespace TinderClone.Migrations
{
    public partial class AddCol_timeStamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "timeStamp",
                table: "Messages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "timeStamp",
                table: "Messages");
        }
    }
}
