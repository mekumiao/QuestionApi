using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyExaminationIdToAnswerHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExaminationId",
                table: "AnswerHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnswerHistories_ExaminationId",
                table: "AnswerHistories",
                column: "ExaminationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerHistories_Examinations_ExaminationId",
                table: "AnswerHistories",
                column: "ExaminationId",
                principalTable: "Examinations",
                principalColumn: "ExaminationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerHistories_Examinations_ExaminationId",
                table: "AnswerHistories");

            migrationBuilder.DropIndex(
                name: "IX_AnswerHistories_ExaminationId",
                table: "AnswerHistories");

            migrationBuilder.DropColumn(
                name: "ExaminationId",
                table: "AnswerHistories");
        }
    }
}
