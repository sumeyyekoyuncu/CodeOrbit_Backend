using CodeOrbit.Application.DTOs.Leaderboard;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _context;

        public LeaderboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int currentUserId, int top = 100)
        {
            var users = await _context.Users
                .Select(u => new
                {
                    User = u,
                    QuizCount = _context.Quizzes.Count(q => q.UserId == u.Id && q.CompletedAt.HasValue),
                    CorrectAnswers = _context.Quizzes.Where(q => q.UserId == u.Id && q.CompletedAt.HasValue).Sum(q => q.CorrectAnswers),
                    TotalQuestions = _context.Quizzes.Where(q => q.UserId == u.Id && q.CompletedAt.HasValue).Sum(q => q.TotalQuestions),
                    BadgeCount = _context.UserBadges.Count(ub => ub.UserId == u.Id),
                    CurrentStreak = _context.UserStreaks.Where(s => s.UserId == u.Id).Select(s => s.CurrentStreak).FirstOrDefault()
                })
                .Where(x => x.QuizCount > 0)
                .ToListAsync();

            var leaderboard = users
                .Select(x => new LeaderboardEntryDto
                {
                    UserId = x.User.Id,
                    Username = x.User.Username,
                    Score = x.QuizCount,
                    SuccessRate = x.TotalQuestions > 0 ? Math.Round((double)x.CorrectAnswers / x.TotalQuestions * 100, 2) : 0,
                    BadgeCount = x.BadgeCount,
                    CurrentStreak = x.CurrentStreak,
                    IsCurrentUser = x.User.Id == currentUserId
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.SuccessRate)
                .Take(top)
                .ToList();

            // Rank ekle
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
            }

            return leaderboard;
        }

        public async Task<List<LeaderboardEntryDto>> GetWeeklyLeaderboardAsync(int currentUserId, int top = 100)
        {
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

            var users = await _context.Users
                .Select(u => new
                {
                    User = u,
                    WeeklyQuizCount = _context.Quizzes.Count(q => q.UserId == u.Id && q.CompletedAt.HasValue && q.CompletedAt >= oneWeekAgo),
                    CorrectAnswers = _context.Quizzes.Where(q => q.UserId == u.Id && q.CompletedAt.HasValue && q.CompletedAt >= oneWeekAgo).Sum(q => q.CorrectAnswers),
                    TotalQuestions = _context.Quizzes.Where(q => q.UserId == u.Id && q.CompletedAt.HasValue && q.CompletedAt >= oneWeekAgo).Sum(q => q.TotalQuestions),
                    BadgeCount = _context.UserBadges.Count(ub => ub.UserId == u.Id),
                    CurrentStreak = _context.UserStreaks.Where(s => s.UserId == u.Id).Select(s => s.CurrentStreak).FirstOrDefault()
                })
                .Where(x => x.WeeklyQuizCount > 0)
                .ToListAsync();

            var leaderboard = users
                .Select(x => new LeaderboardEntryDto
                {
                    UserId = x.User.Id,
                    Username = x.User.Username,
                    Score = x.WeeklyQuizCount,
                    SuccessRate = x.TotalQuestions > 0 ? Math.Round((double)x.CorrectAnswers / x.TotalQuestions * 100, 2) : 0,
                    BadgeCount = x.BadgeCount,
                    CurrentStreak = x.CurrentStreak,
                    IsCurrentUser = x.User.Id == currentUserId
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.SuccessRate)
                .Take(top)
                .ToList();

            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
            }

            return leaderboard;
        }

        public async Task<List<LeaderboardEntryDto>> GetStreakLeaderboardAsync(int currentUserId, int top = 100)
        {
            var users = await _context.UserStreaks
                .Include(s => s.User)
                .Where(s => s.CurrentStreak > 0)
                .OrderByDescending(s => s.CurrentStreak)
                .ThenByDescending(s => s.LongestStreak)
                .Take(top)
                .ToListAsync();

            var leaderboard = new List<LeaderboardEntryDto>();

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var quizCount = await _context.Quizzes.CountAsync(q => q.UserId == user.UserId && q.CompletedAt.HasValue);
                var badgeCount = await _context.UserBadges.CountAsync(ub => ub.UserId == user.UserId);

                var quizzes = await _context.Quizzes
                    .Where(q => q.UserId == user.UserId && q.CompletedAt.HasValue)
                    .ToListAsync();

                var successRate = quizzes.Any()
                    ? Math.Round(quizzes.Average(q => (double)q.CorrectAnswers / q.TotalQuestions) * 100, 2)
                    : 0;

                leaderboard.Add(new LeaderboardEntryDto
                {
                    Rank = i + 1,
                    UserId = user.UserId,
                    Username = user.User.Username,
                    Score = user.CurrentStreak,
                    SuccessRate = successRate,
                    BadgeCount = badgeCount,
                    CurrentStreak = user.CurrentStreak,
                    IsCurrentUser = user.UserId == currentUserId
                });
            }

            return leaderboard;
        }

        public async Task<CategoryLeaderboardDto> GetCategoryLeaderboardAsync(int categoryId, int currentUserId, int top = 50)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                return new CategoryLeaderboardDto { CategoryName = null, Entries = new() };
            // Kategoriye ait soruları çözen kullanıcılar
            var userStats = await _context.QuizQuestions
                .Where(qq => qq.Question.CategoryId == categoryId && qq.IsCorrect.HasValue)
                .GroupBy(qq => qq.Quiz.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalQuestions = g.Count(),
                    CorrectAnswers = g.Count(qq => qq.IsCorrect == true)
                })
                .Where(x => x.TotalQuestions > 0)
                .ToListAsync();

            var entries = new List<LeaderboardEntryDto>();

            foreach (var stat in userStats)
            {
                var user = await _context.Users.FindAsync(stat.UserId);
                var badgeCount = await _context.UserBadges.CountAsync(ub => ub.UserId == stat.UserId);
                var streak = await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == stat.UserId);

                entries.Add(new LeaderboardEntryDto
                {
                    UserId = stat.UserId,
                    Username = user?.Username ?? "Unknown",
                    Score = stat.CorrectAnswers,
                    SuccessRate = Math.Round((double)stat.CorrectAnswers / stat.TotalQuestions * 100, 2),
                    BadgeCount = badgeCount,
                    CurrentStreak = streak?.CurrentStreak ?? 0,
                    IsCurrentUser = stat.UserId == currentUserId
                });
            }

            entries = entries
                .OrderByDescending(e => e.Score)
                .ThenByDescending(e => e.SuccessRate)
                .Take(top)
                .ToList();

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Rank = i + 1;
            }

            return new CategoryLeaderboardDto
            {
                CategoryName = category.Name,
                Entries = entries
            };
        }

        public async Task<List<CategoryLeaderboardDto>> GetAllCategoryLeaderboardsAsync(int currentUserId)
        {
            var categories = await _context.Categories.ToListAsync();
            var result = new List<CategoryLeaderboardDto>();

            foreach (var category in categories)
            {
                var leaderboard = await GetCategoryLeaderboardAsync(category.Id, currentUserId, 10);
                result.Add(leaderboard);
            }

            return result;
        }
    }
}