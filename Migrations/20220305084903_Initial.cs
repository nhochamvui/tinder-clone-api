using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TinderClone.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MyId = table.Column<long>(type: "bigint", nullable: false),
                    ObjectId = table.Column<long>(type: "bigint", nullable: false),
                    IsMatched = table.Column<bool>(type: "boolean", nullable: false),
                    IsDislike = table.Column<bool>(type: "boolean", nullable: false),
                    DateOfMatch = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    About = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscoverySettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    DistancePreference = table.Column<int>(type: "integer", nullable: false),
                    DistancePreferenceCheck = table.Column<bool>(type: "boolean", nullable: false),
                    LookingForGender = table.Column<int>(type: "integer", nullable: false),
                    MinAge = table.Column<int>(type: "integer", nullable: false),
                    MaxAge = table.Column<int>(type: "integer", nullable: false),
                    AgePreferenceCheck = table.Column<bool>(type: "boolean", nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    SuperlikeCount = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fromID = table.Column<long>(type: "bigint", nullable: false),
                    toID = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    isRead = table.Column<bool>(type: "boolean", nullable: false),
                    isSent = table.Column<bool>(type: "boolean", nullable: false),
                    timeStamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Users_fromID",
                        column: x => x.fromID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_toID",
                        column: x => x.toID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileImages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImageURL = table.Column<string>(type: "text", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileImages_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    About = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    UserID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profiles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "About", "DateOfBirth", "Email", "Gender", "Location", "Name", "Password", "UserName" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(1998, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "uncle.bob@gmail.com", 0, "Hồ Chí Minh", "Uncle Bob", "1234", "unclebob" },
                    { 2L, null, new DateTime(1999, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "aunt.bob@gmail.com", 1, "Cần Thơ", "Aunt Bob", "1234", "auntbob" }
                });

            migrationBuilder.InsertData(
                table: "DiscoverySettings",
                columns: new[] { "Id", "AgePreferenceCheck", "DistancePreference", "DistancePreferenceCheck", "LikeCount", "Location", "LookingForGender", "MaxAge", "MinAge", "SuperlikeCount", "UserID" },
                values: new object[,]
                {
                    { 1L, true, 1, true, 100, "Hồ Chí Minh", 1, 100, 18, 4, 1L },
                    { 2L, true, 1, true, 100, "Hồ Chí Minh", 0, 100, 18, 4, 2L }
                });

            migrationBuilder.InsertData(
                table: "ProfileImages",
                columns: new[] { "Id", "ImageURL", "UserID" },
                values: new object[,]
                {
                    { 1L, "img/unclebob.jpg", 1L },
                    { 2L, "img/unclebob1.jpg", 1L },
                    { 3L, "img/auntbob.jpg", 2L }
                });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "Id", "About", "DateOfBirth", "Email", "Gender", "Location", "Name", "Phone", "UserID" },
                values: new object[,]
                {
                    { 1L, "", new DateTime(1998, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "a@gmail.com", 0, "Hồ Chí Minh", "Tho", "0907904598", 1L },
                    { 2L, "", new DateTime(1998, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "a1@gmail.com", 1, "Hồ Chí Minh", "Jan", "0907904598", 2L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscoverySettings_UserID",
                table: "DiscoverySettings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_fromID",
                table: "Messages",
                column: "fromID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_toID",
                table: "Messages",
                column: "toID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileImages_UserID",
                table: "ProfileImages",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserID",
                table: "Profiles",
                column: "UserID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscoverySettings");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "ProfileImages");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
