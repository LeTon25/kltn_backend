using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropAnnouncementIdInComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Announcement_AnnouncementId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_AnnouncementId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "AnnouncementId",
                table: "Comment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnnouncementId",
                table: "Comment",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_AnnouncementId",
                table: "Comment",
                column: "AnnouncementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Announcement_AnnouncementId",
                table: "Comment",
                column: "AnnouncementId",
                principalTable: "Announcement",
                principalColumn: "AnnouncementId");
        }
    }
}
