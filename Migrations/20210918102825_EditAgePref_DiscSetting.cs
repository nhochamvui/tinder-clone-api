using Microsoft.EntityFrameworkCore.Migrations;

namespace TinderClone.Migrations
{
    public partial class EditAgePref_DiscSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LookingForGender",
                table: "DiscoverySettings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "DiscoverySettings",
                keyColumn: "Id",
                keyValue: 1L,
                column: "LookingForGender",
                value: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LookingForGender",
                table: "DiscoverySettings",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "DiscoverySettings",
                keyColumn: "Id",
                keyValue: 1L,
                column: "LookingForGender",
                value: "Female");
        }
    }
}
