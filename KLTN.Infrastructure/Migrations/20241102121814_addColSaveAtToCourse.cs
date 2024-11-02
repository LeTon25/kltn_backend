using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addColSaveAtToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SaveAt",
                table: "Course",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaveAt",
                table: "Course");
        }
    }
}
