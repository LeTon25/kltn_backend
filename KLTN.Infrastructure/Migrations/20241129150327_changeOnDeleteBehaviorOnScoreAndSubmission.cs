using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changeOnDeleteBehaviorOnScoreAndSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Score_Submission_SubmissionId",
                table: "Score");

            migrationBuilder.AddForeignKey(
                name: "FK_Score_Submission_SubmissionId",
                table: "Score",
                column: "SubmissionId",
                principalTable: "Submission",
                principalColumn: "SubmissionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Score_Submission_SubmissionId",
                table: "Score");

            migrationBuilder.AddForeignKey(
                name: "FK_Score_Submission_SubmissionId",
                table: "Score",
                column: "SubmissionId",
                principalTable: "Submission",
                principalColumn: "SubmissionId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
