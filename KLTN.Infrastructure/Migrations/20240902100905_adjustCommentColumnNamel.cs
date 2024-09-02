using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adjustCommentColumnNamel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `Comment` CHANGE `AnnoucementId` `AnnouncementId` VARCHAR;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `Comment` CHANGE `AnnouncementId` `AnnoucementId` VARCHAR;");
        }
    }
}
