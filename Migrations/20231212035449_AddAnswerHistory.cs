using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAnswerHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnswerHistory",
                columns: table => new
                {
                    AnswerHistoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmissionTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerHistory", x => x.AnswerHistoryId);
                    table.ForeignKey(
                        name: "FK_AnswerHistory_Exam_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exam",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnswerHistory_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswer_AnswerHistoryId",
                table: "StudentAnswer",
                column: "AnswerHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerHistory_ExamId",
                table: "AnswerHistory",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerHistory_StudentId",
                table: "AnswerHistory",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAnswer_AnswerHistory_AnswerHistoryId",
                table: "StudentAnswer",
                column: "AnswerHistoryId",
                principalTable: "AnswerHistory",
                principalColumn: "AnswerHistoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentAnswer_AnswerHistory_AnswerHistoryId",
                table: "StudentAnswer");

            migrationBuilder.DropTable(
                name: "AnswerHistory");

            migrationBuilder.DropIndex(
                name: "IX_StudentAnswer_AnswerHistoryId",
                table: "StudentAnswer");
        }
    }
}
