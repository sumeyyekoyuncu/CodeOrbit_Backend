using CodeOrbit.Application.DTOs.Challenge;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Helpers
{
    public static class ChallengeServiceTestHelpers
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

        /// <summary>
        /// Challenge oluşturulabilmesi için minimum 10 soru gerekiyor.
        /// </summary>
        public static async Task<List<Question>> SeedQuestionsAsync(
            AppDbContext context,
            int categoryId,
            DifficultyLevel difficulty = DifficultyLevel.Easy,
            int count = 10)
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

        public static async Task<User> SeedUserAsync(
            AppDbContext context,
            string username = "testuser",
            string email = "test@example.com")
        {
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public static async Task<DailyChallenge> SeedDailyChallengeAsync(
            AppDbContext context,
            int categoryId,
            List<Question> questions,
            DateTime? date = null)
        {
            var challenge = new DailyChallenge
            {
                Date = date?.Date ?? DateTime.UtcNow.Date,
                CategoryId = categoryId,
                DifficultyLevel = DifficultyLevel.Easy,
                Questions = questions.Select((q, i) => new DailyChallengeQuestion
                {
                    QuestionId = q.Id,
                    OrderNumber = i + 1
                }).ToList()
            };

            context.DailyChallenges.Add(challenge);
            await context.SaveChangesAsync();
            return challenge;
        }

        public static async Task<UserChallengeAttempt> SeedAttemptAsync(
            AppDbContext context,
            int userId,
            int challengeId,
            int correctAnswers = 7,
            int totalQuestions = 10)
        {
            var attempt = new UserChallengeAttempt
            {
                UserId = userId,
                DailyChallengeId = challengeId,
                CorrectAnswers = correctAnswers,
                TotalQuestions = totalQuestions,
                CompletedAt = DateTime.UtcNow
            };

            context.UserChallengeAttempts.Add(attempt);
            await context.SaveChangesAsync();
            return attempt;
        }

        /// <summary>
        /// SubmitChallengeAsync için tüm soruların doğru cevabını içeren DTO üretir.
        /// </summary>
        public static SubmitChallengeDto BuildSubmitDto(
            int userId,
            int challengeId,
            List<Question> questions,
            bool allCorrect = true)
        {
            return new SubmitChallengeDto
            {
                UserId = userId,
                DailyChallengeId = challengeId,
                Answers = questions.Select(q => new ChallengeAnswerDto
                {
                    QuestionId = q.Id,
                    SelectedOptionId = allCorrect
                        ? q.Options.First(o => o.IsCorrect).Id
                        : q.Options.First(o => !o.IsCorrect).Id
                }).ToList()
            };
        }
    }
}