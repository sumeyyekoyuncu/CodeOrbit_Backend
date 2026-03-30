using CodeOrbit.Application.DTOs.Challenge;
using CodeOrbit.Application.DTOs.Notification;
using CodeOrbit.Application.DTOs.Question;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class ChallengeService : IChallengeService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public ChallengeService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<DailyChallengeDto> GetTodaysChallengeAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var challenge = await _context.DailyChallenges
                .Include(c => c.Category)
                .Include(c => c.Questions)
                    .ThenInclude(q => q.Question)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(c => c.Date == today);

            if (challenge == null)
            {
                await GenerateDailyChallengeAsync();
                challenge = await _context.DailyChallenges
                    .Include(c => c.Category)
                    .Include(c => c.Questions)
                        .ThenInclude(q => q.Question)
                            .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(c => c.Date == today);
            }

            if (challenge == null)
                throw new Exception("Challenge oluşturulamadı.");

            var hasCompleted = await _context.UserChallengeAttempts
                .AnyAsync(a => a.UserId == userId && a.DailyChallengeId == challenge.Id);

            return new DailyChallengeDto
            {
                ChallengeId = challenge.Id,
                Date = challenge.Date,
                CategoryName = challenge.Category.Name,
                DifficultyLevel = challenge.DifficultyLevel.ToString(),
                TotalQuestions = challenge.Questions.Count,
                HasCompleted = hasCompleted,
                Questions = challenge.Questions
                    .OrderBy(q => q.OrderNumber)
                    .Select(cq => new ChallengeQuestionDto
                    {
                        QuestionId = cq.Question.Id,
                        QuestionText = cq.Question.QuestionText,
                        QuestionType = cq.Question.QuestionType.ToString(),
                        Options = cq.Question.Options.Select(o => new OptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText
                        }).ToList()
                    }).ToList()
            };
        }

        public async Task<ChallengeResultDto> SubmitChallengeAsync(SubmitChallengeDto dto)
        {
            var existing = await _context.UserChallengeAttempts
                .FirstOrDefaultAsync(a => a.UserId == dto.UserId && a.DailyChallengeId == dto.DailyChallengeId);

            if (existing != null)
                throw new Exception("Bu challenge'ı zaten tamamladınız.");

            var correctCount = 0;
            var answers = new List<UserChallengeAnswer>();

            foreach (var answer in dto.Answers)
            {
                var option = await _context.Options.FindAsync(answer.SelectedOptionId);
                if (option != null && option.IsCorrect)
                    correctCount++;

                answers.Add(new UserChallengeAnswer
                {
                    QuestionId = answer.QuestionId,
                    SelectedOptionId = answer.SelectedOptionId,
                    IsCorrect = option?.IsCorrect ?? false
                });
            }

            var attempt = new UserChallengeAttempt
            {
                UserId = dto.UserId,
                DailyChallengeId = dto.DailyChallengeId,
                CorrectAnswers = correctCount,
                TotalQuestions = dto.Answers.Count,
                CompletedAt = DateTime.UtcNow,
                Answers = answers
            };

            _context.UserChallengeAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            var allAttempts = await _context.UserChallengeAttempts
                .Where(a => a.DailyChallengeId == dto.DailyChallengeId)
                .OrderByDescending(a => a.CorrectAnswers)
                .ThenBy(a => a.CompletedAt)
                .ToListAsync();

            var rank = allAttempts.FindIndex(a => a.Id == attempt.Id) + 1;

            return new ChallengeResultDto
            {
                CorrectAnswers = correctCount,
                TotalQuestions = dto.Answers.Count,
                SuccessRate = Math.Round((double)correctCount / dto.Answers.Count * 100, 2),
                Rank = rank,
                TotalParticipants = allAttempts.Count
            };
        }

        public async Task<List<ChallengeLeaderboardDto>> GetTodaysLeaderboardAsync()
        {
            var today = DateTime.UtcNow.Date;

            var challenge = await _context.DailyChallenges
                .FirstOrDefaultAsync(c => c.Date == today);

            if (challenge == null)
                return new List<ChallengeLeaderboardDto>();

            var attempts = await _context.UserChallengeAttempts
                .Where(a => a.DailyChallengeId == challenge.Id)
                .Include(a => a.User)
                .OrderByDescending(a => a.CorrectAnswers)
                .ThenBy(a => a.CompletedAt)
                .Take(100)
                .ToListAsync();

            // Select with index SQL'e çevrilemiyor, client-side'da yapıyoruz
            var leaderboard = attempts.Select((a, index) => new ChallengeLeaderboardDto
            {
                Rank = index + 1,
                Username = a.User.Username,
                CorrectAnswers = a.CorrectAnswers,
                TotalQuestions = a.TotalQuestions,
                SuccessRate = Math.Round((double)a.CorrectAnswers / a.TotalQuestions * 100, 2),
                CompletedAt = a.CompletedAt
            }).ToList();

            return leaderboard;

        }

        public async Task GenerateDailyChallengeAsync()
        {
            var today = DateTime.UtcNow.Date;

            var exists = await _context.DailyChallenges.AnyAsync(c => c.Date == today);
            if (exists) return;

            var random = new Random();
            var categories = await _context.Categories.ToListAsync();
            var randomCategory = categories[random.Next(categories.Count)];

            var difficulties = Enum.GetValues<DifficultyLevel>();
            var randomDifficulty = difficulties[random.Next(difficulties.Length)];

            var questions = await _context.Questions
                .Where(q => q.CategoryId == randomCategory.Id && q.DifficultyLevel == randomDifficulty)
                .OrderBy(q => Guid.NewGuid())
                .Take(10)
                .ToListAsync();

            if (questions.Count < 10)
            {
                questions = await _context.Questions
                    .Where(q => q.CategoryId == randomCategory.Id)
                    .OrderBy(q => Guid.NewGuid())
                    .Take(10)
                    .ToListAsync();
            }

            if (questions.Count < 10)
                throw new Exception("Yeterli soru yok, challenge oluşturulamadı.");

            var challenge = new DailyChallenge
            {
                Date = today,
                CategoryId = randomCategory.Id,
                DifficultyLevel = randomDifficulty,
                Questions = questions.Select((q, index) => new DailyChallengeQuestion
                {
                    QuestionId = q.Id,
                    OrderNumber = index + 1
                }).ToList()
            };

            _context.DailyChallenges.Add(challenge);
            await _context.SaveChangesAsync();

            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = user.Id,
                    Type = NotificationType.DailyChallenge,
                    Title = "🎯 Yeni günlük challenge!",
                    Message = $"Bugünün challenge'ı: {randomCategory.Name} - {randomDifficulty}",
                    ActionUrl = "/challenge"
                });
            }
        }
    }
}