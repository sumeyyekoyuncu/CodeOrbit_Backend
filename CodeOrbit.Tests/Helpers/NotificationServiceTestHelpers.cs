using CodeOrbit.Application.DTOs.Notification;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Helpers
{
    public static class NotificationServiceTestHelpers
    {
        public static async Task<User> SeedUserAsync(
            AppDbContext context,
            string username = "testuser",
            string email = "test@test.com")
        {
            var user = new User { Username = username, Email = email, PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public static async Task<Notification> SeedNotificationAsync(
            AppDbContext context,
            int userId,
            bool isRead = false,
            NotificationType type = NotificationType.FriendRequest,
            DateTime? createdAt = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = "Test bildirimi",
                Message = "Test mesajı",
                IsRead = isRead,
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
            return notification;
        }

        public static async Task SeedUserStreakAsync(
            AppDbContext context,
            int userId,
            int currentStreak,
            DateTime lastActiveDate)
        {
            context.UserStreaks.Add(new UserStreak
            {
                UserId = userId,
                CurrentStreak = currentStreak,
                LongestStreak = currentStreak,
                LastActiveDate = lastActiveDate
            });
            await context.SaveChangesAsync();
        }

        public static async Task SeedUserActivityAsync(
            AppDbContext context,
            int userId,
            DateTime date)
        {
            context.UserActivities.Add(new UserActivity
            {
                UserId = userId,
                Date = date,
                QuestionsSolved = 5,
                LastActivityAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        public static CreateNotificationDto BuildCreateDto(
            int userId,
            NotificationType type = NotificationType.FriendRequest,
            string title = "Başlık",
            string message = "Mesaj") =>
            new CreateNotificationDto
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = "/test"
            };
    }
}