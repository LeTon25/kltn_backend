using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addColTypeToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignmentId",
                table: "Group",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GroupType",
                table: "Group",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Group_AssignmentId",
                table: "Group",
                column: "AssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_Assignment_AssignmentId",
                table: "Group",
                column: "AssignmentId",
                principalTable: "Assignment",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Group_Assignment_AssignmentId",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Group_AssignmentId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "GroupType",
                table: "Group");
        }
    }
}
