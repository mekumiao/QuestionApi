using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations {
    /// <inheritdoc />
    public partial class UpdateTableNameExamsToExamPapers : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerHistories_Exams_ExamId",
                table: "AnswerHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Exams_ExamId",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Questions_QuestionId",
                table: "ExamQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exams",
                table: "Exams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamQuestions",
                table: "ExamQuestions");

            migrationBuilder.RenameTable(
                name: "Exams",
                newName: "ExamPapers");

            migrationBuilder.RenameTable(
                name: "ExamQuestions",
                newName: "ExamPaperQuestions");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestions_QuestionId",
                table: "ExamPaperQuestions",
                newName: "IX_ExamPaperQuestions_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamPapers",
                table: "ExamPapers",
                column: "ExamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamPaperQuestions",
                table: "ExamPaperQuestions",
                columns: new[] { "ExamId", "QuestionId" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPaperQuestions_Questions_QuestionId",
                table: "ExamPaperQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerHistories_ExamPapers_ExamId",
                table: "AnswerHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamPaperQuestions_ExamPapers_ExamId",
                table: "ExamPaperQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamPaperQuestions_Questions_QuestionId",
                table: "ExamPaperQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamPapers",
                table: "ExamPapers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamPaperQuestions",
                table: "ExamPaperQuestions");

            migrationBuilder.RenameTable(
                name: "ExamPapers",
                newName: "Exams");

            migrationBuilder.RenameTable(
                name: "ExamPaperQuestions",
                newName: "ExamQuestions");

            migrationBuilder.RenameIndex(
                name: "IX_ExamPaperQuestions_QuestionId",
                table: "ExamQuestions",
                newName: "IX_ExamQuestions_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exams",
                table: "Exams",
                column: "ExamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamQuestions",
                table: "ExamQuestions",
                columns: new[] { "ExamId", "QuestionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerHistories_Exams_ExamId",
                table: "AnswerHistories",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ExamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Exams_ExamId",
                table: "ExamQuestions",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ExamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Questions_QuestionId",
                table: "ExamQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}