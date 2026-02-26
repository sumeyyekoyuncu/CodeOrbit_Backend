using CodeOrbit.Application.DTOs.Notification;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,
                ActionUrl = dto.ActionUrl,
                RelatedEntityId = dto.RelatedEntityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ActionUrl = n.ActionUrl
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CheckAndSendStreakWarningsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            // Dün aktif olan ama bugün henüz quiz çözmemiş kullanıcılar
            var streaks = await _context.UserStreaks
                .Where(s => s.LastActiveDate == yesterday && s.CurrentStreak > 0)
                .Include(s => s.User)
                .ToListAsync();

            foreach (var streak in streaks)
            {
                // Bugün aktivite var mı kontrol et
                var todayActivity = await _context.UserActivities
                    .AnyAsync(a => a.UserId == streak.UserId && a.Date == today);

                if (!todayActivity)
                {
                    // Daha önce bugün uyarı gönderilmiş mi?
                    var alreadyWarned = await _context.Notifications
                        .AnyAsync(n => n.UserId == streak.UserId &&
                                      n.Type == Domain.Enums.NotificationType.StreakWarning &&
                                      n.CreatedAt.Date == today);

                    if (!alreadyWarned)
                    {
                        await CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = streak.UserId,
                            Type = Domain.Enums.NotificationType.StreakWarning,
                            Title = "🔥 Streak'in kırılmak üzere!",
                            Message = $"{streak.CurrentStreak} günlük serinizi kaybetmemek için bugün bir quiz çözün!",
                            ActionUrl = "/quiz"
                        });
                    }
                }
            }
        }
    }
}