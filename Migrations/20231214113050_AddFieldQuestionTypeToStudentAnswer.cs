using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldQuestionTypeToStudentAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentAnswers",
                table: "StudentAnswers");

            migrationBuilder.DropColumn(
                name: "ChosenOptions",
                table: "StudentAnswers");

            migrationBuilder.RenameColumn(
                name: "AnswerId",
                table: "StudentAnswers",
                newName: "QuestionType");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionType",
                table: "StudentAnswers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "StudentAnswerId",
                table: "StudentAnswers",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentAnswers",
                table: "StudentAnswers",
                column: "StudentAnswerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentAnswers",
                table: "StudentAnswers");

            migrationBuilder.DropColumn(
                name: "StudentAnswerId",
                table: "StudentAnswers");

            migrationBuilder.RenameColumn(
                name: "QuestionType",
                table: "StudentAnswers",
                newName: "AnswerId");

            migrationBuilder.AlterColumn<int>(
                name: "AnswerId",
                table: "StudentAnswers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "ChosenOptions",
                table: "StudentAnswers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentAnswers",
                table: "StudentAnswers",
                column: "AnswerId");
        }
    }
}
