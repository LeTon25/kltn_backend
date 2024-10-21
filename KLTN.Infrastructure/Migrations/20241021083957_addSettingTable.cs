using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addSettingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    SettingId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CourseId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartGroupCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndGroupCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AllowStudentCreateProject = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowGroupRegistration = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxGroupSize = table.Column<int>(type: "int", nullable: true),
                    MinGroupSize = table.Column<int>(type: "int", nullable: true),
                    HasFinalScore = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_Setting_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Setting_CourseId",
                table: "Setting",
                column: "CourseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Setting");
        }
    }
}
