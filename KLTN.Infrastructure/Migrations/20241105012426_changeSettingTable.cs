using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changeSettingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartGroupCreation",
                table: "Setting");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDateToJoinGroup",
                table: "Setting",
                type: "datetime(6)",
                nullable: true
                );
            migrationBuilder.DropColumn(
                name: "AllowGroupRegistration",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "EndGroupCreation",
                table: "Setting");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartGroupCreation",
                table: "Setting",
                type: "datetime(6)",
                nullable:true
                );

            migrationBuilder.AddColumn<bool>(
                name: "AllowGroupRegistration",
                table: "Setting",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndGroupCreation",
                table: "Setting",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "DueDateToJoinGroup",
                table: "Setting");
        }
    }
}
