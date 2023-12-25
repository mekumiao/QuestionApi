using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class AlterForeignKeyExaminationIdToSetNullFromAnswerHistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerHistories_Examinations_ExaminationId",
                table: "AnswerHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerHistories_Examinations_ExaminationId",
                table: "AnswerHistories",
                column: "ExaminationId",
                principalTable: "Examinations",
                principalColumn: "ExaminationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerHistories_Examinations_ExaminationId",
                table: "AnswerHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerHistories_Examinations_ExaminationId",
                table: "AnswerHistories",
                column: "ExaminationId",
                principalTable: "Examinations",
                principalColumn: "ExaminationId");
        }
    }
}
