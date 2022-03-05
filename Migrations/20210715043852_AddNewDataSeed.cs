using Microsoft.EntityFrameworkCore.Migrations;

namespace TinderClone.Migrations
{
    public partial class AddNewDataSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ProfileImages",
                columns: new[] { "Id", "ImageURL", "UserID" },
                values: new object[,]
                {
                    { 1L, "img/unclebob.jpg", 1L },
                    { 2L, "img/unclebob1.jpg", 1L },
                    { 3L, "img/auntbob.jpg", 2L }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProfileImages",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "ProfileImages",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "ProfileImages",
                keyColumn: "Id",
                keyValue: 3L);
        }
    }
}
