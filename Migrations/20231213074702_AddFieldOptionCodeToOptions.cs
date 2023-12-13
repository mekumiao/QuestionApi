using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldOptionCodeToOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<char>(
                name: "OptionCode",
                table: "Options",
                type: "character(1)",
                nullable: false,
                defaultValue: 'A');
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionCode",
                table: "Options");
        }
    }
}
