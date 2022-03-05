using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TinderClone.Migrations
{
    public partial class AddNewModelDiscoverySetting_SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscoverySettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    DistancePreference = table.Column<int>(type: "integer", nullable: false),
                    LookingForGender = table.Column<string>(type: "text", nullable: true),
                    MinAge = table.Column<int>(type: "integer", nullable: false),
                    MaxAge = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscoverySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscoverySettings_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DiscoverySettings",
                columns: new[] { "Id", "DistancePreference", "Location", "LookingForGender", "MaxAge", "MinAge", "UserID" },
                values: new object[] { 1L, 1, "Hồ Chí Minh", "Female", 25, 18, 1L });

            migrationBuilder.CreateIndex(
                name: "IX_DiscoverySettings_UserID",
                table: "DiscoverySettings",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscoverySettings");
        }
    }
}
