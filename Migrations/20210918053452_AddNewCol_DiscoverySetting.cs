using Microsoft.EntityFrameworkCore.Migrations;

namespace TinderClone.Migrations
{
    public partial class AddNewCol_DiscoverySetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AgePreferenceCheck",
                table: "DiscoverySettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DistancePreferenceCheck",
                table: "DiscoverySettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgePreferenceCheck",
                table: "DiscoverySettings");

            migrationBuilder.DropColumn(
                name: "DistancePreferenceCheck",
                table: "DiscoverySettings");
        }
    }
}
