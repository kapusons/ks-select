using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KsSelect.Samples.Migrations
{
    /// <inheritdoc />
    public partial class AddBookLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "BooksLocalization",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BooksLocalization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BooksLocalization_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BooksLocalization",
                columns: new[] { "Id", "BookId", "Description", "Language", "Title" },
                values: new object[,]
                {
                    { 1L, 1L, "descrizione in italiano libro 1", "IT", "Titolo italiano libro 1" },
                    { 2L, 2L, "descrizione in italiano libro 2", "IT", "Titolo italiano libro 2" },
                    { 3L, 3L, "descrizione in italiano libro 3", "IT", "Titolo italiano libro 3" },
                    { 4L, 4L, "descrizione in italiano libro 4", "IT", "Titolo italiano libro 4" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BooksLocalization_BookId",
                table: "BooksLocalization",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BooksLocalization");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
