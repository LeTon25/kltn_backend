using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Announcement_AnnouncementId",
                table: "Comment");

            migrationBuilder.DropTable(
                name: "ReportComment");

            migrationBuilder.AlterColumn<string>(
                name: "AnnouncementId",
                table: "Comment",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CommentableId",
                table: "Comment",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CommentableType",
                table: "Comment",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Assignment",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_UserId",
                table: "Assignment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignment_AspNetUsers_UserId",
                table: "Assignment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Announcement_AnnouncementId",
                table: "Comment",
                column: "AnnouncementId",
                principalTable: "Announcement",
                principalColumn: "AnnouncementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignment_AspNetUsers_UserId",
                table: "Assignment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Announcement_AnnouncementId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Assignment_UserId",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "CommentableId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "CommentableType",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Assignment");

            migrationBuilder.UpdateData(
                table: "Comment",
                keyColumn: "AnnouncementId",
                keyValue: null,
                column: "AnnouncementId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "AnnouncementId",
                table: "Comment",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReportComment",
                columns: table => new
                {
                    ReportCommentId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportComment", x => x.ReportCommentId);
                    table.ForeignKey(
                        name: "FK_ReportComment_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportComment_Report_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Report",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComment_ReportId",
                table: "ReportComment",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComment_UserId",
                table: "ReportComment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Announcement_AnnouncementId",
                table: "Comment",
                column: "AnnouncementId",
                principalTable: "Announcement",
                principalColumn: "AnnouncementId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
