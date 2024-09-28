using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeColInReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Report");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Report",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Report",
                newName: "Mentions");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Report",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Report",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
