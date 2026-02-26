using CodeOrbit.Application.DTOs.Statistics;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync(int userId)
        {
            // Kullanıcının tamamladığı quizler
            var completedQuizzes = await _context.Quizzes
                .Where(q => q.UserId == userId && q.CompletedAt.HasValue)
                .Include(q => q.Category)
                .ToListAsync();

            // Kullanıcının cevapladığı tüm sorular
            var answeredQuestions = await _context.QuizQuestions
                .Where(qq => qq.Quiz.UserId == userId && qq.IsCorrect.HasValue)
                .Include(qq => qq.Question)
                    .ThenInclude(q => q.Category)
                .Include(qq => qq.Quiz)
                .ToListAsync();

            // Genel istatistikler
            var totalQuestions = answeredQuestions.Count;
            var totalCorrect = answeredQuestions.Count(qq => qq.IsCorrect == true);
            var totalWrong = answeredQuestions.Count(qq => qq.IsCorrect == false);

            // Kategori bazlı istatistikler
            var categoryStats = answeredQuestions
                .GroupBy(qq => qq.Question.Category.Name)
                .Select(g => new CategoryStatDto
                {
                    CategoryName = g.Key,
                    QuestionsSolved = g.Count(),
                    CorrectAnswers = g.Count(qq => qq.IsCorrect == true),
                    SuccessRate = g.Any() ? Math.Round((double)g.Count(qq => qq.IsCorrect == true) / g.Count() * 100, 2) : 0
                })
                .OrderByDescending(c => c.SuccessRate)
                .ToList();

            // Zorluk seviyesi bazlı istatistikler
            var difficultyStats = answeredQuestions
                .GroupBy(qq => qq.Question.DifficultyLevel)
                .Select(g => new DifficultyStatDto
                {
                    DifficultyLevel = g.Key.ToString(),
                    QuestionsSolved = g.Count(),
                    CorrectAnswers = g.Count(qq => qq.IsCorrect == true),
                    SuccessRate = g.Any() ? Math.Round((double)g.Count(qq => qq.IsCorrect == true) / g.Count() * 100, 2) : 0
                })
                .OrderBy(d => d.DifficultyLevel)
                .ToList();

            // En çok yanlış yapılan sorular
            var mostWrongQuestions = answeredQuestions
                .GroupBy(qq => new { qq.QuestionId, qq.Question.QuestionText, qq.Question.Category.Name })
                .Select(g => new
                {
                    g.Key.QuestionId,
                    g.Key.QuestionText,
                    g.Key.Name,
                    TimesAnswered = g.Count(),
                    TimesWrong = g.Count(qq => qq.IsCorrect == false)
                })
                .Where(x => x.TimesWrong > 0)
                .OrderByDescending(x => x.TimesWrong)
                .Take(10)
                .Select(x => new MostWrongQuestionDto
                {
                    QuestionId = x.QuestionId,
                    QuestionText = x.QuestionText,
                    CategoryName = x.Name,
                    TimesAnswered = x.TimesAnswered,
                    TimesWrong = x.TimesWrong,
                    WrongRate = Math.Round((double)x.TimesWrong / x.TimesAnswered * 100, 2)
                })
                .ToList();

            return new UserStatisticsDto
            {
                TotalQuizzes = completedQuizzes.Count,
                TotalQuestionsSolved = totalQuestions,
                TotalCorrectAnswers = totalCorrect,
                TotalWrongAnswers = totalWrong,
                OverallSuccessRate = totalQuestions > 0 ? Math.Round((double)totalCorrect / totalQuestions * 100, 2) : 0,
                CategoryStats = categoryStats,
                DifficultyStats = difficultyStats,
                MostWrongQuestions = mostWrongQuestions
            };
        }
    }
}