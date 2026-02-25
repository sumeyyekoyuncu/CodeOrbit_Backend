using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeOrbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Options_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProgresses_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Language", "Name" },
                values: new object[,]
                {
                    { 1, 0, "C#" },
                    { 2, 1, "Java" },
                    { 3, 2, "Kotlin" },
                    { 4, 3, "Python" },
                    { 5, 4, "JavaScript" }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CategoryId", "DifficultyLevel", "QuestionText", "QuestionType" },
                values: new object[,]
                {
                    { 1, 1, 0, "C#'ta değişken nasıl tanımlanır?", 0 },
                    { 2, 1, 1, "C#'ta LINQ nedir?", 0 },
                    { 3, 1, 0, "C# bir nesne yönelimli dildir.", 1 },
                    { 4, 1, 1, "C#'ta bir listeyi tanımlamak için ____<int> list = new() kullanılır.", 2 },
                    { 5, 1, 0, "C#'ta try bloğundan sonra gelen blok...", 3 },
                    { 6, 2, 1, "Java'da interface nedir?", 0 },
                    { 7, 2, 2, "Java'da generic nedir?", 0 },
                    { 8, 2, 1, "Java'da çoklu kalıtım doğrudan desteklenir.", 1 },
                    { 9, 2, 0, "Java'da sabit tanımlamak için ____ anahtar kelimesi kullanılır.", 2 },
                    { 10, 2, 0, "Java'da bir sınıfın başka bir sınıftan türemesi için...", 3 },
                    { 11, 3, 0, "Kotlin'de değişken nasıl tanımlanır?", 0 },
                    { 12, 3, 1, "Kotlin'de null kontrolü nasıl yapılır?", 0 },
                    { 13, 3, 0, "Kotlin, Java ile %100 uyumludur.", 1 },
                    { 14, 3, 0, "Kotlin'de değiştirilemez değişken tanımlamak için ____ kullanılır.", 2 },
                    { 15, 4, 0, "Python'da liste nasıl tanımlanır?", 0 },
                    { 16, 4, 0, "Python yorumlanan bir dildir.", 1 },
                    { 17, 4, 0, "Python'da fonksiyon tanımlamak için ____ anahtar kelimesi kullanılır.", 2 },
                    { 18, 5, 0, "JavaScript'te değişken tanımlamak için hangi anahtar kelimeler kullanılır?", 0 },
                    { 19, 5, 1, "JavaScript yalnızca tarayıcıda çalışır.", 1 },
                    { 20, 5, 0, "JavaScript'te bir fonksiyon tanımlamak için ____ anahtar kelimesi kullanılır.", 2 }
                });

            migrationBuilder.InsertData(
                table: "Options",
                columns: new[] { "Id", "IsCorrect", "OptionText", "QuestionId" },
                values: new object[,]
                {
                    { 1, true, "int x = 5;", 1 },
                    { 2, false, "var x = 5;", 1 },
                    { 3, false, "let x = 5;", 1 },
                    { 4, false, "x := 5;", 1 },
                    { 5, true, "Veri sorgulama yöntemi", 2 },
                    { 6, false, "Yeni sınıf oluşturma", 2 },
                    { 7, false, "Exception handling", 2 },
                    { 8, false, "GUI tasarımı", 2 },
                    { 9, true, "Doğru", 3 },
                    { 10, false, "Yanlış", 3 },
                    { 11, true, "List", 4 },
                    { 12, true, "catch veya finally bloğu gelir", 5 },
                    { 13, false, "else bloğu gelir", 5 },
                    { 14, false, "void bloğu gelir", 5 },
                    { 15, true, "Sınıfların şablonu", 6 },
                    { 16, false, "Yalnızca değişken tutar", 6 },
                    { 17, false, "Exception türü", 6 },
                    { 18, false, "Sadece static metodlar içerir", 6 },
                    { 19, true, "Tip güvenli veri yapısı", 7 },
                    { 20, false, "Exception handling yöntemi", 7 },
                    { 21, false, "GUI tasarım yöntemi", 7 },
                    { 22, false, "Sadece class", 7 },
                    { 23, false, "Doğru", 8 },
                    { 24, true, "Yanlış", 8 },
                    { 25, true, "final", 9 },
                    { 26, true, "extends anahtar kelimesi kullanılır", 10 },
                    { 27, false, "implements anahtar kelimesi kullanılır", 10 },
                    { 28, false, "inherits anahtar kelimesi kullanılır", 10 },
                    { 29, true, "val x = 5", 11 },
                    { 30, false, "int x = 5", 11 },
                    { 31, false, "let x = 5", 11 },
                    { 32, false, "var x: Int = 5", 11 },
                    { 33, true, "?.", 12 },
                    { 34, false, "?", 12 },
                    { 35, false, "!!", 12 },
                    { 36, false, "null", 12 },
                    { 37, true, "Doğru", 13 },
                    { 38, false, "Yanlış", 13 },
                    { 39, true, "val", 14 },
                    { 40, true, "list = []", 15 },
                    { 41, false, "list = {}", 15 },
                    { 42, false, "list = ()", 15 },
                    { 43, false, "list = <>", 15 },
                    { 44, true, "Doğru", 16 },
                    { 45, false, "Yanlış", 16 },
                    { 46, true, "def", 17 },
                    { 47, true, "var, let, const", 18 },
                    { 48, false, "int, string, bool", 18 },
                    { 49, false, "val, var", 18 },
                    { 50, false, "define, set", 18 },
                    { 51, false, "Doğru", 19 },
                    { 52, true, "Yanlış", 19 },
                    { 53, true, "function", 20 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Options_QuestionId",
                table: "Options",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CategoryId",
                table: "Questions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_QuestionId",
                table: "UserProgresses",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_UserId",
                table: "UserProgresses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "UserProgresses");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
