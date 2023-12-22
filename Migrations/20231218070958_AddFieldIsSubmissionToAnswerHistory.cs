using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations {
    /// <inheritdoc />
    public partial class AddFieldIsSubmissionToAnswerHistory : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubmission",
                table: "AnswerHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalIncorrectAnswers",
                table: "AnswerHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "IsSubmission",
                table: "AnswerHistories");

            migrationBuilder.DropColumn(
                name: "TotalIncorrectAnswers",
                table: "AnswerHistories");
        }
    }
}