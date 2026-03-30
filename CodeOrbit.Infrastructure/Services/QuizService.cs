using CodeOrbit.Application.DTOs.Question;
using CodeOrbit.Application.DTOs.Quiz;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly IActivityService _activityService;
        private readonly IBadgeService _badgeService;

        public QuizService(AppDbContext context, IActivityService activityService, IBadgeService badgeService)
        {
            _context = context;
            _activityService = activityService;
            _badgeService = badgeService;
        }

        public async Task<QuizDto> StartQuizAsync(StartQuizDto dto)
        {
            IQueryable<Question> query = _context.Questions
                .Where(q => q.CategoryId == dto.CategoryId && q.DifficultyLevel == dto.DifficultyLevel)
                .Include(q => q.Options)
                .Include(q => q.Category);

            if (dto.FromFavoritesOnly)
            {
                var favoriteQuestionIds = await _context.FavoriteQuestions
                    .Where(f => f.UserId == dto.UserId)
                    .Select(f => f.QuestionId)
                    .ToListAsync();

                query = query.Where(q => favoriteQuestionIds.Contains(q.Id));
            }

            var questions = await query
                .OrderBy(q => Guid.NewGuid())
                .Take(dto.QuestionCount)
                .ToListAsync();

            if (questions.Count < dto.QuestionCount)
                throw new Exception($"Yeterli soru bulunamadı. İstenen: {dto.QuestionCount}, Bulunan: {questions.Count}");

            var quiz = new Quiz
            {
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                DifficultyLevel = dto.DifficultyLevel,
                TotalQuestions = questions.Count,
                CorrectAnswers = 0
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            var quizQuestions = questions.Select(q => new QuizQuestion
            {
                QuizId = quiz.Id,
                QuestionId = q.Id
            }).ToList();

            _context.QuizQuestions.AddRange(quizQuestions);
            await _context.SaveChangesAsync();

            return new QuizDto
            {
                QuizId = quiz.Id,
                CategoryName = questions.First().Category.Name,
                DifficultyLevel = dto.DifficultyLevel.ToString(),
                TotalQuestions = quiz.TotalQuestions,
                Questions = quizQuestions.Select((qq, index) =>
                {
                    var question = questions[index];
                    return new QuizQuestionDto
                    {
                        QuizQuestionId = qq.Id,
                        QuestionId = question.Id,
                        QuestionText = question.QuestionText,
                        QuestionType = question.QuestionType.ToString(),
                        Options = question.Options.Select(o => new OptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText
                        }).ToList()
                    };
                }).ToList()
            };
        }

        public async Task<bool> SubmitAnswerAsync(SubmitQuizAnswerDto dto)
        {
            var quizQuestion = await _context.QuizQuestions
                .Include(qq => qq.Question)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(qq => qq.Id == dto.QuizQuestionId && qq.QuizId == dto.QuizId);

            if (quizQuestion == null) return false;

            var selectedOption = quizQuestion.Question.Options
                .FirstOrDefault(o => o.Id == dto.SelectedOptionId);

            if (selectedOption == null) return false;

            quizQuestion.UserAnswerOptionId = dto.SelectedOptionId;
            quizQuestion.IsCorrect = selectedOption.IsCorrect;

            if (selectedOption.IsCorrect)
            {
                var quiz = await _context.Quizzes.FindAsync(dto.QuizId);
                if (quiz != null)
                    quiz.CorrectAnswers++;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<QuizResultDto> CompleteQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                throw new Exception("Quiz bulunamadı.");

            if (quiz.CompletedAt.HasValue)
                throw new Exception("Bu quiz zaten tamamlanmış.");

            quiz.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _activityService.UpdateActivityAsync(quiz.UserId, quiz.TotalQuestions);
            await _badgeService.CheckAndAwardBadgesAsync(quiz.UserId);

            return new QuizResultDto
            {
                QuizId = quiz.Id,
                TotalQuestions = quiz.TotalQuestions,
                CorrectAnswers = quiz.CorrectAnswers,
                WrongAnswers = quiz.TotalQuestions - quiz.CorrectAnswers,
                SuccessRate = Math.Round((double)quiz.CorrectAnswers / quiz.TotalQuestions * 100, 2),
                CompletedAt = quiz.CompletedAt.Value
            };
        }

        public async Task<List<QuizHistoryDto>> GetUserQuizHistoryAsync(int userId)
        {
            return await _context.Quizzes
                .Where(q => q.UserId == userId && q.CompletedAt.HasValue)
                .Include(q => q.Category)
                .OrderByDescending(q => q.CompletedAt)
                .Select(q => new QuizHistoryDto
                {
                    QuizId = q.Id,
                    CategoryName = q.Category.Name,
                    DifficultyLevel = q.DifficultyLevel.ToString(),
                    CorrectAnswers = q.CorrectAnswers,
                    TotalQuestions = q.TotalQuestions,
                    SuccessRate = Math.Round((double)q.CorrectAnswers / q.TotalQuestions * 100, 2),
                    CompletedAt = q.CompletedAt!.Value
                })
                .ToListAsync();
        }
    }
}