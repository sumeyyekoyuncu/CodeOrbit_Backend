using CodeOrbit.Application.DTOs.Favorite;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Helpers
{
    public static class FavoriteServiceTestHelpers
    {
        public static async Task<Question> SeedQuestionAsync(
            AppDbContext context,
            string questionText = "Test sorusu",
            DifficultyLevel difficulty = DifficultyLevel.Easy)
        {
            var category = new Category { Name = "Test Kategori", Language = ProgrammingLanguage.CSharp };
            context.Categories.Add(category);

            var question = new Question
            {
                CategoryId = category.Id,
                QuestionText = questionText,
                DifficultyLevel = difficulty,
                QuestionType = QuestionType.MultipleChoice,
                Options = new List<Option>
                {
                    new Option { OptionText = "A", IsCorrect = true },
                    new Option { OptionText = "B", IsCorrect = false }
                }
            };
            context.Questions.Add(question);
            await context.SaveChangesAsync();
            return question;
        }

        public static async Task<FavoriteQuestion> SeedFavoriteAsync(
            AppDbContext context,
            int userId,
            int questionId)
        {
            var favorite = new FavoriteQuestion
            {
                UserId = userId,
                QuestionId = questionId,
                AddedAt = DateTime.UtcNow
            };
            context.FavoriteQuestions.Add(favorite);
            await context.SaveChangesAsync();
            return favorite;
        }

        public static AddFavoriteDto BuildAddFavoriteDto(int userId, int questionId) =>
            new AddFavoriteDto { UserId = userId, QuestionId = questionId };
    }
}