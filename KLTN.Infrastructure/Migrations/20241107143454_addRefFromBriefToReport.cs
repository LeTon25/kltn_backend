using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addRefFromBriefToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportId",
                table: "Brief",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Brief_ReportId",
                table: "Brief",
                column: "ReportId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Brief_Report_ReportId",
                table: "Brief",
                column: "ReportId",
                principalTable: "Report",
                principalColumn: "ReportId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Brief_Report_ReportId",
                table: "Brief");

            migrationBuilder.DropIndex(
                name: "IX_Brief_ReportId",
                table: "Brief");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "Brief");
        }
    }
}
