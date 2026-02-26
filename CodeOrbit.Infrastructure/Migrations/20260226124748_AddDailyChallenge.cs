using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeOrbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyChallenges_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyChallengeQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyChallengeId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyChallengeQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyChallengeQuestions_DailyChallenges_DailyChallengeId",
                        column: x => x.DailyChallengeId,
                        principalTable: "DailyChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyChallengeQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserChallengeAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DailyChallengeId = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChallengeAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChallengeAttempts_DailyChallenges_DailyChallengeId",
                        column: x => x.DailyChallengeId,
                        principalTable: "DailyChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChallengeAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChallengeAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserChallengeAttemptId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChallengeAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChallengeAnswers_Options_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "Options",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserChallengeAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserChallengeAnswers_UserChallengeAttempts_UserChallengeAttemptId",
                        column: x => x.UserChallengeAttemptId,
                        principalTable: "UserChallengeAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallengeQuestions_DailyChallengeId",
                table: "DailyChallengeQuestions",
                column: "DailyChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallengeQuestions_QuestionId",
                table: "DailyChallengeQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_CategoryId",
                table: "DailyChallenges",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeAnswers_QuestionId",
                table: "UserChallengeAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeAnswers_SelectedOptionId",
                table: "UserChallengeAnswers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeAnswers_UserChallengeAttemptId",
                table: "UserChallengeAnswers",
                column: "UserChallengeAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeAttempts_DailyChallengeId",
                table: "UserChallengeAttempts",
                column: "DailyChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeAttempts_UserId_DailyChallengeId",
                table: "UserChallengeAttempts",
                columns: new[] { "UserId", "DailyChallengeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyChallengeQuestions");

            migrationBuilder.DropTable(
                name: "UserChallengeAnswers");

            migrationBuilder.DropTable(
                name: "UserChallengeAttempts");

            migrationBuilder.DropTable(
                name: "DailyChallenges");
        }
    }
}
