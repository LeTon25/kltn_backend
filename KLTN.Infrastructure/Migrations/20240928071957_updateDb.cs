using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Semester_SemesterId",
                table: "Course");

            migrationBuilder.DropTable(
                name: "Semester");

            migrationBuilder.DropIndex(
                name: "IX_Course_SemesterId",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "Course");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Report",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Semester",
                table: "Course",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "Course");

            migrationBuilder.AddColumn<string>(
                name: "SemesterId",
                table: "Course",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Semester",
                columns: table => new
                {
                    SemesterId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semester", x => x.SemesterId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Course_SemesterId",
                table: "Course",
                column: "SemesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Semester_SemesterId",
                table: "Course",
                column: "SemesterId",
                principalTable: "Semester",
                principalColumn: "SemesterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
