using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldsRelatedToExamPaper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerHistories_ExamPapers_ExamId",
                table: "AnswerHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamPaperQuestions_ExamPapers_ExamId",
                table: "ExamPaperQuestions");

            migrationBuilder.RenameColumn(
                name: "ExamName",
                table: "ExamPapers",
                newName: "ExamPaperName");

            migrationBuilder.RenameColumn(
                name: "ExamId",
                table: "ExamPapers",
                newName: "ExamPaperId");

            migrationBuilder.RenameColumn(
                name: "ExamId",
                table: "ExamPaperQuestions",
                newName: "ExamPaperId");

            migrationBuilder.RenameColumn(
                name: "ExamId",
                table: "AnswerHistories",
                newName: "ExamPaperId");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerHistories_ExamId",
                table: "AnswerHistories",
                newName: "IX_AnswerHistories_ExamPaperId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerHistories_ExamPapers_ExamPaperId",
                table: "AnswerHistories",
                column: "ExamPaperId",
                principalTable: "ExamPapers",
                principalColumn: "ExamPaperId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPaperQuestions_ExamPapers_ExamPaperId",
                table: "ExamPaperQuestions",
                column: "ExamPaperId",
                principalTable: "ExamPapers",
                principalColumn: "ExamPaperId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerHistories_ExamPapers_ExamPaperId",
                table: "AnswerHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamPaperQuestions_ExamPapers_ExamPaperId",
                table: "ExamPaperQuestions");

            migrationBuilder.RenameColumn(
                name: "ExamPaperName",
                table: "ExamPapers",
                newName: "ExamName");

            migrationBuilder.RenameColumn(
                name: "ExamPaperId",
                table: "ExamPapers",
                newName: "ExamId");

            migrationBuilder.RenameColumn(
                name: "ExamPaperId",
                table: "ExamPaperQuestions",
                newName: "ExamId");

            migrationBuilder.RenameColumn(
                name: "ExamPaperId",
                table: "AnswerHistories",
                newName: "ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerHistories_ExamPaperId",
                table: "AnswerHistories",
                newName: "IX_AnswerHistories_ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerHistories_ExamPapers_ExamId",
                table: "AnswerHistories",
                column: "ExamId",
                principalTable: "ExamPapers",
                principalColumn: "ExamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPaperQuestions_ExamPapers_ExamId",
                table: "ExamPaperQuestions",
                column: "ExamId",
                principalTable: "ExamPapers",
                principalColumn: "ExamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
