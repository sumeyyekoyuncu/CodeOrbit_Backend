using CodeOrbit.Application.DTOs.Quiz;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using CodeOrbit.Infrastructure.Services;
using CodeOrbit.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CodeOrbit.Tests.Services
{
    public class QuizServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly Mock<IActivityService> _activityMock;
        private readonly Mock<IBadgeService> _badgeMock;
        private readonly QuizService _sut;

        public QuizServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _activityMock = new Mock<IActivityService>();
            _badgeMock = new Mock<IBadgeService>();
            _sut = new QuizService(_context, _activityMock.Object, _badgeMock.Object);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region StartQuizAsync

        [Fact]
        public async Task StartQuizAsync_SufficientQuestions_ReturnsQuizDto()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 5);

            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };

            // Act
            var result = await _sut.StartQuizAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalQuestions);
            Assert.Equal(3, result.Questions.Count);
            Assert.Equal(category.Name, result.CategoryName);
        }

        [Fact]
        public async Task StartQuizAsync_SufficientQuestions_PersistsQuizToDatabase()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 5);

            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };

            // Act
            var result = await _sut.StartQuizAsync(dto);

            // Assert
            var savedQuiz = await _context.Quizzes.FindAsync(result.QuizId);
            Assert.NotNull(savedQuiz);
            Assert.Equal(1, savedQuiz.UserId);
            Assert.Equal(3, savedQuiz.TotalQuestions);
            Assert.Equal(0, savedQuiz.CorrectAnswers);
        }

        [Fact]
        public async Task StartQuizAsync_SufficientQuestions_PersistsQuizQuestionsToDatabase()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 5);

            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };

            // Act
            var result = await _sut.StartQuizAsync(dto);

            // Assert
            var quizQuestions = await _context.QuizQuestions
                .Where(qq => qq.QuizId == result.QuizId)
                .ToListAsync();
            Assert.Equal(3, quizQuestions.Count);
        }

        [Fact]
        public async Task StartQuizAsync_InsufficientQuestions_ThrowsException()
        {
            // Arrange — 2 soru var, 5 isteniyor
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 2);

            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 5
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.StartQuizAsync(dto));
            Assert.Contains("Yeterli soru bulunamadı", ex.Message);
        }

        [Fact]
        public async Task StartQuizAsync_FromFavoritesOnly_ReturnsOnlyFavoriteQuestions()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 5);

            // Sadece ilk 2 soruyu favoriye ekle
            _context.FavoriteQuestions.Add(new Domain.Entities.FavoriteQuestion { UserId = 1, QuestionId = questions[0].Id });
            _context.FavoriteQuestions.Add(new Domain.Entities.FavoriteQuestion { UserId = 1, QuestionId = questions[1].Id });
            await _context.SaveChangesAsync();

            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 2,
                FromFavoritesOnly = true
            };

            // Act
            var result = await _sut.StartQuizAsync(dto);

            // Assert
            var returnedIds = result.Questions.Select(q => q.QuestionId).ToList();
            Assert.All(returnedIds, id => Assert.Contains(id, new[] { questions[0].Id, questions[1].Id }));
        }

        [Fact]
        public async Task StartQuizAsync_FromFavoritesOnly_InsufficientFavorites_ThrowsException()
        {
            // Arrange — 5 soru var ama sadece 1 favori
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 5);

            _context.FavoriteQuestions.Add(new Domain.Entities.FavoriteQuestion { UserId = 1, QuestionId = questions[0].Id });
            await _context.SaveChangesAsync();

            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3,
                FromFavoritesOnly = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.StartQuizAsync(dto));
        }

        #endregion

        #region SubmitAnswerAsync

        [Fact]
        public async Task SubmitAnswerAsync_CorrectAnswer_ReturnsTrueAndIncrementsCorrectAnswers()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 1);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id);
            var qq = await QuizServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz.Id, questions[0].Id);

            var correctOptionId = questions[0].Options.First(o => o.IsCorrect).Id;

            var dto = new SubmitQuizAnswerDto
            {
                QuizId = quiz.Id,
                QuizQuestionId = qq.Id,
                SelectedOptionId = correctOptionId
            };

            // Act
            var result = await _sut.SubmitAnswerAsync(dto);

            // Assert
            Assert.True(result);
            var updatedQuiz = await _context.Quizzes.FindAsync(quiz.Id);
            Assert.Equal(4, updatedQuiz!.CorrectAnswers); // seed'den 3 + 1
        }

        [Fact]
        public async Task SubmitAnswerAsync_WrongAnswer_ReturnsTrueButDoesNotIncrementCorrectAnswers()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 1);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id);
            var qq = await QuizServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz.Id, questions[0].Id);

            var wrongOptionId = questions[0].Options.First(o => !o.IsCorrect).Id;

            var dto = new SubmitQuizAnswerDto
            {
                QuizId = quiz.Id,
                QuizQuestionId = qq.Id,
                SelectedOptionId = wrongOptionId
            };

            // Act
            var result = await _sut.SubmitAnswerAsync(dto);

            // Assert
            Assert.True(result);
            var updatedQuiz = await _context.Quizzes.FindAsync(quiz.Id);
            Assert.Equal(3, updatedQuiz!.CorrectAnswers); // değişmemeli
        }

        [Fact]
        public async Task SubmitAnswerAsync_InvalidQuizQuestionId_ReturnsFalse()
        {
            // Arrange
            var dto = new SubmitQuizAnswerDto
            {
                QuizId = 999,
                QuizQuestionId = 999,
                SelectedOptionId = 1
            };

            // Act
            var result = await _sut.SubmitAnswerAsync(dto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SubmitAnswerAsync_InvalidOptionId_ReturnsFalse()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await QuizServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 1);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id);
            var qq = await QuizServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz.Id, questions[0].Id);

            var dto = new SubmitQuizAnswerDto
            {
                QuizId = quiz.Id,
                QuizQuestionId = qq.Id,
                SelectedOptionId = 99999 // olmayan option
            };

            // Act
            var result = await _sut.SubmitAnswerAsync(dto);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CompleteQuizAsync

        [Fact]
        public async Task CompleteQuizAsync_ValidQuiz_ReturnsCorrectResult()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(
                _context, userId: 1, categoryId: category.Id,
                totalQuestions: 10, correctAnswers: 7);

            // Act
            var result = await _sut.CompleteQuizAsync(quiz.Id);

            // Assert
            Assert.Equal(10, result.TotalQuestions);
            Assert.Equal(7, result.CorrectAnswers);
            Assert.Equal(3, result.WrongAnswers);
            Assert.Equal(70.0, result.SuccessRate);
        }

        [Fact]
        public async Task CompleteQuizAsync_ValidQuiz_SetsCompletedAt()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id);

            // Act
            await _sut.CompleteQuizAsync(quiz.Id);

            // Assert
            var updatedQuiz = await _context.Quizzes.FindAsync(quiz.Id);
            Assert.NotNull(updatedQuiz!.CompletedAt);
        }

        [Fact]
        public async Task CompleteQuizAsync_ValidQuiz_CallsUpdateActivity()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(
                _context, userId: 1, categoryId: category.Id, totalQuestions: 5);

            // Act
            await _sut.CompleteQuizAsync(quiz.Id);

            // Assert
            _activityMock.Verify(
                a => a.UpdateActivityAsync(1, 5),
                Times.Once
            );
        }

        [Fact]
        public async Task CompleteQuizAsync_ValidQuiz_CallsCheckAndAwardBadges()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id);

            // Act
            await _sut.CompleteQuizAsync(quiz.Id);

            // Assert
            _badgeMock.Verify(
                b => b.CheckAndAwardBadgesAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task CompleteQuizAsync_QuizNotFound_ThrowsException()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.CompleteQuizAsync(999));
            Assert.Equal("Quiz bulunamadı.", ex.Message);
        }

        [Fact]
        public async Task CompleteQuizAsync_AlreadyCompleted_ThrowsException()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(
                _context, userId: 1, categoryId: category.Id, completed: true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.CompleteQuizAsync(quiz.Id));
            Assert.Equal("Bu quiz zaten tamamlanmış.", ex.Message);
        }

        [Fact]
        public async Task CompleteQuizAsync_AlreadyCompleted_DoesNotCallActivityOrBadgeService()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            var quiz = await QuizServiceTestHelpers.SeedQuizAsync(
                _context, userId: 1, categoryId: category.Id, completed: true);

            // Act
            await Assert.ThrowsAsync<Exception>(() => _sut.CompleteQuizAsync(quiz.Id));

            // Assert — hata fırlatıldıysa downstream servisler çağrılmamalı
            _activityMock.Verify(a => a.UpdateActivityAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _badgeMock.Verify(b => b.CheckAndAwardBadgesAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region GetUserQuizHistoryAsync

        [Fact]
        public async Task GetUserQuizHistoryAsync_NoCompletedQuizzes_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetUserQuizHistoryAsync(userId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserQuizHistoryAsync_ReturnsOnlyCompletedQuizzes()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id, completed: true);
            await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id, completed: true);
            await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id, completed: false); // tamamlanmamış

            // Act
            var result = await _sut.GetUserQuizHistoryAsync(userId: 1);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUserQuizHistoryAsync_ReturnsOnlyCurrentUsersQuizzes()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 1, categoryId: category.Id, completed: true);
            await QuizServiceTestHelpers.SeedQuizAsync(_context, userId: 2, categoryId: category.Id, completed: true); // başka user

            // Act
            var result = await _sut.GetUserQuizHistoryAsync(userId: 1);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetUserQuizHistoryAsync_CalculatesSuccessRateCorrectly()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);
            await QuizServiceTestHelpers.SeedQuizAsync(
                _context, userId: 1, categoryId: category.Id,
                totalQuestions: 10, correctAnswers: 8, completed: true);

            // Act
            var result = await _sut.GetUserQuizHistoryAsync(userId: 1);

            // Assert
            Assert.Equal(80.0, result[0].SuccessRate);
        }

        [Fact]
        public async Task GetUserQuizHistoryAsync_ReturnsOrderedByCompletedAtDescending()
        {
            // Arrange
            var category = await QuizServiceTestHelpers.SeedCategoryAsync(_context);

            var older = new Domain.Entities.Quiz
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                TotalQuestions = 5,
                CorrectAnswers = 3,
                CompletedAt = DateTime.UtcNow.AddDays(-2)
            };
            var newer = new Domain.Entities.Quiz
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                TotalQuestions = 5,
                CorrectAnswers = 4,
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            };
            _context.Quizzes.AddRange(older, newer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserQuizHistoryAsync(userId: 1);

            // Assert — en yeni önce gelmeli
            Assert.True(result[0].CompletedAt > result[1].CompletedAt);
        }

        #endregion
    }
}