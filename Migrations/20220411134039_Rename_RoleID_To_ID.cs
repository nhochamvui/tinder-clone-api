using Microsoft.EntityFrameworkCore.Migrations;

namespace TinderClone.Migrations
{
    public partial class Rename_RoleID_To_ID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleUser_Role_RolesRoleID",
                table: "RoleUser");

            migrationBuilder.RenameColumn(
                name: "RolesRoleID",
                table: "RoleUser",
                newName: "RolesId");

            migrationBuilder.RenameColumn(
                name: "RoleID",
                table: "Role",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleUser_Role_RolesId",
                table: "RoleUser",
                column: "RolesId",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleUser_Role_RolesId",
                table: "RoleUser");

            migrationBuilder.RenameColumn(
                name: "RolesId",
                table: "RoleUser",
                newName: "RolesRoleID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Role",
                newName: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleUser_Role_RolesRoleID",
                table: "RoleUser",
                column: "RolesRoleID",
                principalTable: "Role",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
