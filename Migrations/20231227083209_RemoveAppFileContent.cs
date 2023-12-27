using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuestionApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAppFileContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppFiles_AppFileContents_ContentId",
                table: "AppFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "AppFileContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppFiles",
                table: "AppFiles");

            migrationBuilder.DropIndex(
                name: "IX_AppFiles_ContentId",
                table: "AppFiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AppFiles");

            migrationBuilder.RenameColumn(
                name: "ContentId",
                table: "AppFiles",
                newName: "FileId");

            migrationBuilder.AlterColumn<int>(
                name: "FileId",
                table: "AppFiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "AppFiles",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppFiles",
                table: "AppFiles",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppFiles",
                table: "AppFiles");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "AppFiles");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "AppFiles",
                newName: "ContentId");

            migrationBuilder.AlterColumn<int>(
                name: "ContentId",
                table: "AppFiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AppFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppFiles",
                table: "AppFiles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AppFileContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFileContents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppFiles_ContentId",
                table: "AppFiles",
                column: "ContentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppFiles_AppFileContents_ContentId",
                table: "AppFiles",
                column: "ContentId",
                principalTable: "AppFileContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
