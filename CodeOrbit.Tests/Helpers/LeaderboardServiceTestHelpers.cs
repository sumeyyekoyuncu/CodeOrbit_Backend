using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Helpers
{
    public static class LeaderboardServiceTestHelpers
    {
        public static async Task<User> SeedUserAsync(
            AppDbContext context,
            string username,
            string email)
        {
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = "hash"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public static async Task<Category> SeedCategoryAsync(
            AppDbContext context,
            string name = "C#")
        {
            var category = new Category { Name = name, Language = ProgrammingLanguage.CSharp };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// Tamamlanmış quiz ekler. completedDaysAgo ile kaç gün önce tamamlandığını belirle.
        /// </summary>
        public static async Task<Quiz> SeedCompletedQuizAsync(
            AppDbContext context,
            int userId,
            int categoryId,
            int totalQuestions = 10,
            int correctAnswers = 7,
            int completedDaysAgo = 0)
        {
            var quiz = new Quiz
            {
                UserId = userId,
                CategoryId = categoryId,
                DifficultyLevel = DifficultyLevel.Easy,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                CompletedAt = DateTime.UtcNow.AddDays(-completedDaysAgo)
            };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();
            return quiz;
        }

        public static async Task SeedUserStreakAsync(
            AppDbContext context,
            int userId,
            int currentStreak,
            int longestStreak = 0)
        {
            context.UserStreaks.Add(new UserStreak
            {
                UserId = userId,
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak == 0 ? currentStreak : longestStreak,
                LastActiveDate = DateTime.UtcNow.Date
            });
            await context.SaveChangesAsync();
        }

        public static async Task SeedUserBadgeAsync(
            AppDbContext context,
            int userId,
            int badgeId)
        {
            context.UserBadges.Add(new UserBadge
            {
                UserId = userId,
                BadgeId = badgeId,
                EarnedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        public static async Task<Badge> SeedBadgeAsync(AppDbContext context, string name = "Test Rozet")
        {
            var badge = new Badge
            {
                Name = name,
                Description = "Test",
                Icon = "🏅",
                Requirement = "Complete1Quiz",
                RequiredCount = 1
            };
            context.Badges.Add(badge);
            await context.SaveChangesAsync();
            return badge;
        }

        /// <summary>
        /// Kategori leaderboard testi için QuizQuestion seed'i.
        /// </summary>
        public static async Task SeedQuizQuestionAsync(
            AppDbContext context,
            int quizId,
            int questionId,
            bool isCorrect)
        {
            context.QuizQuestions.Add(new QuizQuestion
            {
                QuizId = quizId,
                QuestionId = questionId,
                IsCorrect = isCorrect
            });
            await context.SaveChangesAsync();
        }

        public static async Task<Question> SeedQuestionAsync(
            AppDbContext context,
            int categoryId)
        {
            var question = new Question
            {
                CategoryId = categoryId,
                QuestionText = "Test sorusu",
                DifficultyLevel = DifficultyLevel.Easy,
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
    }
}