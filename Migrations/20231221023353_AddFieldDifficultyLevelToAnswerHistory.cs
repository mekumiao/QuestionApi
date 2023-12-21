using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldDifficultyLevelToAnswerHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DifficultyLevel",
                table: "AnswerHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "AnswerHistories");
        }
    }
}
