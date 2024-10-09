using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addBrief : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Score_Submission_SubmissionId",
                table: "Score");

            migrationBuilder.AlterColumn<string>(
                name: "SubmissionId",
                table: "Score",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Brief",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GroupId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brief", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brief_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Brief_GroupId",
                table: "Brief",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Score_Submission_SubmissionId",
                table: "Score",
                column: "SubmissionId",
                principalTable: "Submission",
                principalColumn: "SubmissionId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Score_Submission_SubmissionId",
                table: "Score");

            migrationBuilder.DropTable(
                name: "Brief");

            migrationBuilder.UpdateData(
                table: "Score",
                keyColumn: "SubmissionId",
                keyValue: null,
                column: "SubmissionId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "SubmissionId",
                table: "Score",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Score_Submission_SubmissionId",
                table: "Score",
                column: "SubmissionId",
                principalTable: "Submission",
                principalColumn: "SubmissionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
