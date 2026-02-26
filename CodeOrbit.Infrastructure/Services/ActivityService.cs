using CodeOrbit.Application.DTOs.Activity;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class ActivityService : IActivityService
    {
        private readonly AppDbContext _context;

        public ActivityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserActivityDto> GetUserActivityAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            // Bugünkü aktivite
            var todayActivity = await _context.UserActivities
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

            var todayCount = todayActivity?.QuestionsSolved ?? 0;

            // Streak bilgisi
            var streak = await _context.UserStreaks
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (streak == null)
            {
                streak = new UserStreak
                {
                    UserId = userId,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    LastActiveDate = today
                };
                _context.UserStreaks.Add(streak);
                await _context.SaveChangesAsync();
            }

            // Son 7 günün aktiviteleri
            var last7Days = await _context.UserActivities
                .Where(a => a.UserId == userId && a.Date >= today.AddDays(-7))
                .OrderByDescending(a => a.Date)
                .Select(a => new DailyActivityDto
                {
                    Date = a.Date,
                    QuestionsSolved = a.QuestionsSolved
                })
                .ToListAsync();

            return new UserActivityDto
            {
                TodayQuestionsSolved = todayCount,
                CurrentStreak = streak.CurrentStreak,
                LongestStreak = streak.LongestStreak,
                LastActiveDate = streak.LastActiveDate,
                Last7Days = last7Days
            };
        }

        public async Task<bool> CanStartQuizAsync(int userId, int questionCount)
        {
            // Limit yok, her zaman true döndür
            return true;
        }

        public async Task UpdateActivityAsync(int userId, int questionsSolved)
        {
            var today = DateTime.UtcNow.Date;

            // Bugünkü aktiviteyi güncelle veya oluştur
            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

            if (activity == null)
            {
                activity = new UserActivity
                {
                    UserId = userId,
                    Date = today,
                    QuestionsSolved = questionsSolved,
                    LastActivityAt = DateTime.UtcNow
                };
                _context.UserActivities.Add(activity);
            }
            else
            {
                activity.QuestionsSolved += questionsSolved;
                activity.LastActivityAt = DateTime.UtcNow;
            }

            // Streak güncelle
            await UpdateStreakAsync(userId, today);

            await _context.SaveChangesAsync();
        }

        private async Task UpdateStreakAsync(int userId, DateTime today)
        {
            var streak = await _context.UserStreaks
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (streak == null)
            {
                streak = new UserStreak
                {
                    UserId = userId,
                    CurrentStreak = 1,
                    LongestStreak = 1,
                    LastActiveDate = today
                };
                _context.UserStreaks.Add(streak);
                return;
            }

            var daysSinceLastActive = (today - streak.LastActiveDate).Days;

            if (daysSinceLastActive == 0)
            {
                // Bugün zaten aktif olmuş, değişiklik yok
                return;
            }
            else if (daysSinceLastActive == 1)
            {
                // Dün de aktifti, streak devam ediyor
                streak.CurrentStreak++;
                if (streak.CurrentStreak > streak.LongestStreak)
                {
                    streak.LongestStreak = streak.CurrentStreak;
                }
            }
            else
            {
                // Streak kırıldı
                streak.CurrentStreak = 1;
            }

            streak.LastActiveDate = today;
        }
    }
}