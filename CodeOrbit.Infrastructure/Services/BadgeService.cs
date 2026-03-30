using CodeOrbit.Application.DTOs.Badge;
using CodeOrbit.Application.DTOs.Notification;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public BadgeService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<BadgeDto>> GetUserBadgesAsync(int userId)
        {
            var allBadges = await _context.Badges.ToListAsync();
            var earnedBadges = await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Badge)
                .ToListAsync();

            var result = new List<BadgeDto>();

            foreach (var badge in allBadges)
            {
                var earned = earnedBadges.FirstOrDefault(ub => ub.BadgeId == badge.Id);
                var progress = await GetProgressAsync(userId, badge.Requirement);

                result.Add(new BadgeDto
                {
                    Id = badge.Id,
                    Name = badge.Name,
                    Description = badge.Description,
                    Icon = badge.Icon,
                    IsEarned = earned != null,
                    EarnedAt = earned?.EarnedAt,
                    Progress = progress,
                    RequiredCount = badge.RequiredCount
                });
            }

            return result;
        }

        public async Task CheckAndAwardBadgesAsync(int userId)
        {
            var badges = await _context.Badges.ToListAsync();
            var earnedBadgeIds = await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.BadgeId)
                .ToListAsync();

            foreach (var badge in badges)
            {
                if (earnedBadgeIds.Contains(badge.Id))
                    continue;

                var progress = await GetProgressAsync(userId, badge.Requirement);

                if (progress >= badge.RequiredCount)
                {
                    _context.UserBadges.Add(new UserBadge
                    {
                        UserId = userId,
                        BadgeId = badge.Id,
                        EarnedAt = DateTime.UtcNow
                    });

                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = userId,
                        Type = NotificationType.FriendAchievement,
                        Title = $"🎉 Yeni rozet kazandın!",
                        Message = $"{badge.Icon} {badge.Name}: {badge.Description}",
                        ActionUrl = "/profile/badges"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<int> GetProgressAsync(int userId, string requirement)
        {
            return requirement switch
            {
                "Complete1Quiz" => await _context.Quizzes.CountAsync(q => q.UserId == userId && q.CompletedAt.HasValue),
                "Complete10Quizzes" => await _context.Quizzes.CountAsync(q => q.UserId == userId && q.CompletedAt.HasValue),
                "Complete50Quizzes" => await _context.Quizzes.CountAsync(q => q.UserId == userId && q.CompletedAt.HasValue),
                "PerfectScore" => await _context.Quizzes.CountAsync(q => q.UserId == userId && q.CorrectAnswers == q.TotalQuestions),
                "Streak7Days" => (await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == userId))?.CurrentStreak ?? 0,
                "Streak30Days" => (await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == userId))?.LongestStreak ?? 0,
                "Have5Friends" => await _context.Friendships.CountAsync(f => f.User1Id == userId || f.User2Id == userId),
                "Complete10Challenges" => await _context.UserChallengeAttempts.CountAsync(a => a.UserId == userId),
                _ => 0
            };
        }
    }
}