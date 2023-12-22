using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionApi.Migrations {
    /// <inheritdoc />
    public partial class UpdateFieldToDurationSeconds : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Examinations");

            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Examinations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Examinations");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "Examinations",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}