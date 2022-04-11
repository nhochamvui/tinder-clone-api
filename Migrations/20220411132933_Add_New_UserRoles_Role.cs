using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TinderClone.Migrations
{
    public partial class Add_New_UserRoles_Role : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RolesRoleID = table.Column<long>(type: "bigint", nullable: false),
                    UsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesRoleID, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Role_RolesRoleID",
                        column: x => x.RolesRoleID,
                        principalTable: "Role",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "DiscoverySettings",
                keyColumn: "Id",
                keyValue: 1L,
                column: "LookingForGender",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DiscoverySettings",
                keyColumn: "Id",
                keyValue: 2L,
                column: "LookingForGender",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Gender", "Hometown" },
                values: new object[] { 2, "Cần Thơ" });

            migrationBuilder.UpdateData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "Gender", "Hometown" },
                values: new object[] { 2, "Cần Thơ" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                table: "RoleUser",
                column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.UpdateData(
                table: "DiscoverySettings",
                keyColumn: "Id",
                keyValue: 1L,
                column: "LookingForGender",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DiscoverySettings",
                keyColumn: "Id",
                keyValue: 2L,
                column: "LookingForGender",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Gender", "Hometown" },
                values: new object[] { 0, null });

            migrationBuilder.UpdateData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "Gender", "Hometown" },
                values: new object[] { 1, null });
        }
    }
}
