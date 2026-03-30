using CodeOrbit.Application.DTOs.Question;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Helpers
{
    public static class QuestionServiceTestHelpers
    {
        public static async Task<Category> SeedCategoryAsync(
            AppDbContext context,
            string name = "C#",
            ProgrammingLanguage language = ProgrammingLanguage.CSharp)
        {
            var category = new Category { Name = name, Language = language };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        public static async Task<Question> SeedQuestionAsync(
            AppDbContext context,
            int categoryId,
            string questionText = "Test sorusu",
            DifficultyLevel difficulty = DifficultyLevel.Easy,
            QuestionType type = QuestionType.MultipleChoice)
        {
            var question = new Question
            {
                CategoryId = categoryId,
                QuestionText = questionText,
                DifficultyLevel = difficulty,
                QuestionType = type,
                Options = new List<Option>
                {
                    new Option { OptionText = "A şıkkı", IsCorrect = true },
                    new Option { OptionText = "B şıkkı", IsCorrect = false },
                    new Option { OptionText = "C şıkkı", IsCorrect = false },
                    new Option { OptionText = "D şıkkı", IsCorrect = false }
                }
            };
            context.Questions.Add(question);
            await context.SaveChangesAsync();
            return question;
        }

        public static CreateQuestionDto BuildCreateDto(
            int categoryId,
            string questionText = "Yeni soru",
            DifficultyLevel difficulty = DifficultyLevel.Easy,
            QuestionType type = QuestionType.MultipleChoice) =>
            new CreateQuestionDto
            {
                CategoryId = categoryId,
                QuestionText = questionText,
                DifficultyLevel = difficulty,
                QuestionType = type,
                Options = new List<CreateOptionDto>
                {
                    new CreateOptionDto { OptionText = "A şıkkı", IsCorrect = true },
                    new CreateOptionDto { OptionText = "B şıkkı", IsCorrect = false }
                }
            };
    }
}