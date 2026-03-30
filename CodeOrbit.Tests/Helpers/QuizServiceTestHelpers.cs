using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Helpers
{
    public static class QuizServiceTestHelpers
    {
        public static async Task<Category> SeedCategoryAsync(AppDbContext context, string name = "C#")
        {
            var category = new Category { Name = name };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        public static async Task<List<Question>> SeedQuestionsAsync(
            AppDbContext context,
            int categoryId,
            DifficultyLevel difficulty,
            int count)
        {
            var questions = new List<Question>();

            for (int i = 0; i < count; i++)
            {
                var question = new Question
                {
                    CategoryId = categoryId,
                    DifficultyLevel = difficulty,
                    QuestionText = $"Soru {i + 1}",
                    QuestionType = QuestionType.MultipleChoice,
                    Options = new List<Option>
                    {
                        new Option { OptionText = "A şıkkı", IsCorrect = true },
                        new Option { OptionText = "B şıkkı", IsCorrect = false },
                        new Option { OptionText = "C şıkkı", IsCorrect = false },
                        new Option { OptionText = "D şıkkı", IsCorrect = false }
                    }
                };
                questions.Add(question);
                context.Questions.Add(question);
            }

            await context.SaveChangesAsync();
            return questions;
        }

        public static async Task<Quiz> SeedQuizAsync(
            AppDbContext context,
            int userId,
            int categoryId,
            int totalQuestions = 5,
            int correctAnswers = 3,
            bool completed = false)
        {
            var quiz = new Quiz
            {
                UserId = userId,
                CategoryId = categoryId,
                DifficultyLevel = DifficultyLevel.Easy,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                CompletedAt = completed ? DateTime.UtcNow : null
            };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();
            return quiz;
        }

        public static async Task<QuizQuestion> SeedQuizQuestionAsync(
            AppDbContext context,
            int quizId,
            int questionId)
        {
            var qq = new QuizQuestion { QuizId = quizId, QuestionId = questionId };
            context.QuizQuestions.Add(qq);
            await context.SaveChangesAsync();
            return qq;
        }
    }
}