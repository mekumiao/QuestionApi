using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Questions",
                newName: "QuestionType");

            migrationBuilder.RenameColumn(
                name: "Remark",
                table: "Questions",
                newName: "QuestionText");

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Option",
                columns: table => new
                {
                    OptionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    OptionText = table.Column<string>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Option", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_Option_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Option_QuestionId",
                table: "Option",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Option");

            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "QuestionType",
                table: "Questions",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "QuestionText",
                table: "Questions",
                newName: "Remark");
        }
    }
}
