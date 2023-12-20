using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldIsTimeoutToAnswerHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "AnswerHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTimeout",
                table: "AnswerHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TimeTakenSeconds",
                table: "AnswerHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "AnswerHistories");

            migrationBuilder.DropColumn(
                name: "IsTimeout",
                table: "AnswerHistories");

            migrationBuilder.DropColumn(
                name: "TimeTakenSeconds",
                table: "AnswerHistories");
        }
    }
}
