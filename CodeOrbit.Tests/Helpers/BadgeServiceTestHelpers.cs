using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Helpers
{
    public static class BadgeServiceTestHelpers
    {
        public static async Task<Badge> SeedBadgeAsync(
            AppDbContext context,
            string name,
            string requirement,
            int requiredCount)
        {
            var badge = new Badge
            {
                Name = name,
                Description = $"{name} açıklaması",
                Icon = "🏅",
                Requirement = requirement,
                RequiredCount = requiredCount
            };
            context.Badges.Add(badge);
            await context.SaveChangesAsync();
            return badge;
        }

        public static async Task SeedCompletedQuizzesAsync(
            AppDbContext context,
            int userId,
            int count,
            bool perfect = false)
        {
            for (int i = 0; i < count; i++)
            {
                context.Quizzes.Add(new Quiz
                {
                    UserId = userId,
                    CompletedAt = DateTime.UtcNow,
                    TotalQuestions = 10,
                    CorrectAnswers = perfect ? 10 : 5
                });
            }
            await context.SaveChangesAsync();
        }

        public static async Task SeedUserStreakAsync(
            AppDbContext context,
            int userId,
            int currentStreak,
            int longestStreak)
        {
            context.UserStreaks.Add(new UserStreak
            {
                UserId = userId,
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                LastActiveDate = DateTime.UtcNow.Date
            });
            await context.SaveChangesAsync();
        }

        public static async Task SeedFriendshipsAsync(
            AppDbContext context,
            int userId,
            int friendCount)
        {
            for (int i = 2; i <= friendCount + 1; i++)
            {
                context.Friendships.Add(new Friendship { User1Id = userId, User2Id = i });
            }
            await context.SaveChangesAsync();
        }

        public static async Task SeedEarnedBadgeAsync(
            AppDbContext context,
            int userId,
            int badgeId)
        {
            context.UserBadges.Add(new UserBadge
            {
                UserId = userId,
                BadgeId = badgeId,
                EarnedAt = DateTime.UtcNow.AddDays(-1)
            });
            await context.SaveChangesAsync();
        }
    }
}