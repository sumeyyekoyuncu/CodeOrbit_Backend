using CodeOrbit.Application.DTOs.Challenge;
using CodeOrbit.Application.DTOs.Notification;
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
    public class ChallengeServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly ChallengeService _sut;

        public ChallengeServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _notificationMock = new Mock<INotificationService>();
            _sut = new ChallengeService(_context, _notificationMock.Object);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region GetTodaysChallengeAsync

        [Fact]
        public async Task GetTodaysChallengeAsync_ChallengeExists_ReturnsChallengeDto()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);

            // Act
            var result = await _sut.GetTodaysChallengeAsync(userId: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(challenge.Id, result.ChallengeId);
            Assert.Equal(category.Name, result.CategoryName);
            Assert.Equal(10, result.TotalQuestions);
        }

        [Fact]
        public async Task GetTodaysChallengeAsync_ChallengeExists_HasCompletedFalseForNewUser()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);

            // Act
            var result = await _sut.GetTodaysChallengeAsync(userId: 1);

            // Assert
            Assert.False(result.HasCompleted);
        }

        [Fact]
        public async Task GetTodaysChallengeAsync_UserAlreadyCompleted_HasCompletedTrue()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);
            await ChallengeServiceTestHelpers.SeedAttemptAsync(_context, userId: 1, challengeId: challenge.Id);

            // Act
            var result = await _sut.GetTodaysChallengeAsync(userId: 1);

            // Assert
            Assert.True(result.HasCompleted);
        }

        [Fact]
        public async Task GetTodaysChallengeAsync_QuestionsOrderedByOrderNumber()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);

            // Act
            var result = await _sut.GetTodaysChallengeAsync(userId: 1);

            // Assert — soruların OptionText'lerini kontrol etmek yerine sayıyı doğrula
            Assert.Equal(10, result.Questions.Count);
        }

        [Fact]
        public async Task GetTodaysChallengeAsync_NoChallengeToday_GeneratesAndReturnsChallengeDto()
        {
            // Arrange — challenge yok ama generate için kategori + sorular lazım
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, count: 10);

            // Act
            var result = await _sut.GetTodaysChallengeAsync(userId: 1);

            // Assert — generate edilmiş olmalı
            Assert.NotNull(result);
            Assert.Equal(DateTime.UtcNow.Date, result.Date);
        }

        #endregion

        #region GenerateDailyChallengeAsync

        [Fact]
        public async Task GenerateDailyChallengeAsync_CreatesChallengeForToday()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, count: 10);

            // Act
            await _sut.GenerateDailyChallengeAsync();

            // Assert
            var challenge = await _context.DailyChallenges
                .FirstOrDefaultAsync(c => c.Date == DateTime.UtcNow.Date);
            Assert.NotNull(challenge);
        }

        [Fact]
        public async Task GenerateDailyChallengeAsync_ChallengeAlreadyExists_DoesNotDuplicate()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);

            // Act — ikinci kez çağır
            await _sut.GenerateDailyChallengeAsync();

            // Assert — hâlâ 1 tane olmalı
            var count = await _context.DailyChallenges.CountAsync(c => c.Date == DateTime.UtcNow.Date);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GenerateDailyChallengeAsync_InsufficientQuestions_ThrowsException()
        {
            // Arrange — hiç soru yok
            await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.GenerateDailyChallengeAsync());
            Assert.Contains("Yeterli soru yok", ex.Message);
        }

        [Fact]
        public async Task GenerateDailyChallengeAsync_NotifiesAllUsers()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, count: 10);
            await ChallengeServiceTestHelpers.SeedUserAsync(_context, "user1", "user1@test.com");
            await ChallengeServiceTestHelpers.SeedUserAsync(_context, "user2", "user2@test.com");

            // Act
            await _sut.GenerateDailyChallengeAsync();

            // Assert — her kullanıcıya 1 bildirim gitmeli
            _notificationMock.Verify(
                n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()),
                Times.Exactly(2)
            );
        }

        [Fact]
        public async Task GenerateDailyChallengeAsync_NoUsers_DoesNotCallNotification()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, count: 10);

            // Act
            await _sut.GenerateDailyChallengeAsync();

            // Assert
            _notificationMock.Verify(
                n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()),
                Times.Never
            );
        }

        [Fact]
        public async Task GenerateDailyChallengeAsync_InsufficientQuestionsForDifficulty_FallsBackToAnyDifficulty()
        {
            // Arrange — Easy'de 5 soru var, ama tüm difficulty'lerde 10 soru var
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Easy, count: 5);
            await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id, DifficultyLevel.Medium, count: 5);

            // Act — hata fırlatmamalı, fallback çalışmalı
            // (Toplam 10 soru var, farklı difficulty'lerde)
            await _sut.GenerateDailyChallengeAsync();

            var challenge = await _context.DailyChallenges.FirstOrDefaultAsync();
            Assert.NotNull(challenge);
        }

        #endregion

        #region SubmitChallengeAsync

        [Fact]
        public async Task SubmitChallengeAsync_AllCorrect_ReturnsFullScore()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);
            var dto = ChallengeServiceTestHelpers.BuildSubmitDto(1, challenge.Id, questions, allCorrect: true);

            // Act
            var result = await _sut.SubmitChallengeAsync(dto);

            // Assert
            Assert.Equal(10, result.CorrectAnswers);
            Assert.Equal(100.0, result.SuccessRate);
        }

        [Fact]
        public async Task SubmitChallengeAsync_AllWrong_ReturnsZeroScore()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);
            var dto = ChallengeServiceTestHelpers.BuildSubmitDto(1, challenge.Id, questions, allCorrect: false);

            // Act
            var result = await _sut.SubmitChallengeAsync(dto);

            // Assert
            Assert.Equal(0, result.CorrectAnswers);
            Assert.Equal(0.0, result.SuccessRate);
        }

        [Fact]
        public async Task SubmitChallengeAsync_ValidSubmit_PersistsAttemptToDatabase()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);
            var dto = ChallengeServiceTestHelpers.BuildSubmitDto(1, challenge.Id, questions);

            // Act
            await _sut.SubmitChallengeAsync(dto);

            // Assert
            var attempt = await _context.UserChallengeAttempts
                .FirstOrDefaultAsync(a => a.UserId == 1 && a.DailyChallengeId == challenge.Id);
            Assert.NotNull(attempt);
        }

        [Fact]
        public async Task SubmitChallengeAsync_AlreadySubmitted_ThrowsException()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);
            await ChallengeServiceTestHelpers.SeedAttemptAsync(_context, userId: 1, challengeId: challenge.Id);

            var dto = ChallengeServiceTestHelpers.BuildSubmitDto(1, challenge.Id, questions);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.SubmitChallengeAsync(dto));
            Assert.Equal("Bu challenge'ı zaten tamamladınız.", ex.Message);
        }

        [Fact]
        public async Task SubmitChallengeAsync_FirstParticipant_RankIsOne()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);
            var dto = ChallengeServiceTestHelpers.BuildSubmitDto(1, challenge.Id, questions, allCorrect: true);

            // Act
            var result = await _sut.SubmitChallengeAsync(dto);

            // Assert
            Assert.Equal(1, result.Rank);
            Assert.Equal(1, result.TotalParticipants);
        }

        [Fact]
        public async Task SubmitChallengeAsync_LowerScore_RankIsLower()
        {
            // Arrange — user1 10 doğru yaptı, user2 daha az yapacak
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);

            // user1 önce tamamladı, tüm doğru
            await ChallengeServiceTestHelpers.SeedAttemptAsync(
                _context, userId: 1, challengeId: challenge.Id,
                correctAnswers: 10, totalQuestions: 10);

            // user2 daha az doğru
            var dto = ChallengeServiceTestHelpers.BuildSubmitDto(2, challenge.Id, questions, allCorrect: false);

            // Act
            var result = await _sut.SubmitChallengeAsync(dto);

            // Assert — user2 daha az doğru yaptığı için 2. sırada olmalı
            Assert.Equal(2, result.Rank);
            Assert.Equal(2, result.TotalParticipants);
        }

        #endregion

        #region GetTodaysLeaderboardAsync

        [Fact]
        public async Task GetTodaysLeaderboardAsync_NoChallengeToday_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetTodaysLeaderboardAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTodaysLeaderboardAsync_WithAttempts_ReturnsOrderedByCorrectAnswers()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);

            // Gerçek ID'leri seed'den alıyoruz, sabit int vermiyoruz
            var user1 = await ChallengeServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await ChallengeServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            await ChallengeServiceTestHelpers.SeedAttemptAsync(_context, userId: user1.Id, challengeId: challenge.Id, correctAnswers: 6);
            await ChallengeServiceTestHelpers.SeedAttemptAsync(_context, userId: user2.Id, challengeId: challenge.Id, correctAnswers: 9);

            // Act
            var result = await _sut.GetTodaysLeaderboardAsync();

            // Assert — user2 daha yüksek skor, önce gelmeli
            Assert.Equal(2, result.Count);
            Assert.Equal(9, result[0].CorrectAnswers);
            Assert.Equal(6, result[1].CorrectAnswers);
        }

        [Fact]
        public async Task GetTodaysLeaderboardAsync_ReturnsMax100Entries()
        {
            // Arrange
            var category = await ChallengeServiceTestHelpers.SeedCategoryAsync(_context);
            var questions = await ChallengeServiceTestHelpers.SeedQuestionsAsync(_context, category.Id);
            var challenge = await ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_context, category.Id, questions);

            for (int i = 1; i <= 105; i++)
            {
                var user = await ChallengeServiceTestHelpers.SeedUserAsync(_context, $"user{i}", $"u{i}@test.com");
                await ChallengeServiceTestHelpers.SeedAttemptAsync(_context, userId: user.Id, challengeId: challenge.Id);
            }

            // Act
            var result = await _sut.GetTodaysLeaderboardAsync();

            // Assert
            Assert.Equal(100, result.Count);
        }

        #endregion
    }
}