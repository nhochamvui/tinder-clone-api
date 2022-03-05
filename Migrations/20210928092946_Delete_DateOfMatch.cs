using Microsoft.EntityFrameworkCore.Migrations;

namespace TinderClone.Migrations
{
    public partial class Delete_DateOfMatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DateOfMatch2",
                table: "Matches",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfMatch2",
                table: "Matches");
        }
    }
}
