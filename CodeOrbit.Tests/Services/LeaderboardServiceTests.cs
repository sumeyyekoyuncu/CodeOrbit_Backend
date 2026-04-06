using CodeOrbit.Application.DTOs.Leaderboard;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Infrastructure.Data;
using CodeOrbit.Infrastructure.Services;
using CodeOrbit.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CodeOrbit.Tests.Services
{
    public class LeaderboardServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly LeaderboardService _sut;

        public LeaderboardServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            var mockCache = new Mock<ILeaderboardCacheService>();
            mockCache.Setup(x => x.GetGlobalLeaderboardAsync()).ReturnsAsync((List<LeaderboardEntryDto>?)null);
            mockCache.Setup(x => x.GetWeeklyLeaderboardAsync()).ReturnsAsync((List<LeaderboardEntryDto>?)null);
            mockCache.Setup(x => x.GetStreakLeaderboardAsync()).ReturnsAsync((List<LeaderboardEntryDto>?)null);
            mockCache.Setup(x => x.SetGlobalLeaderboardAsync(It.IsAny<List<LeaderboardEntryDto>>())).Returns(Task.CompletedTask);
            mockCache.Setup(x => x.SetWeeklyLeaderboardAsync(It.IsAny<List<LeaderboardEntryDto>>())).Returns(Task.CompletedTask);
            mockCache.Setup(x => x.SetStreakLeaderboardAsync(It.IsAny<List<LeaderboardEntryDto>>())).Returns(Task.CompletedTask);

            _sut = new LeaderboardService(_context, mockCache.Object);
        }
        public void Dispose() => _context.Dispose();

        #endregion

        #region GetGlobalLeaderboardAsync

        [Fact]
        public async Task GetGlobalLeaderboardAsync_NoUsers_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGlobalLeaderboardAsync_ExcludesUsersWithNoCompletedQuizzes()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "active", "active@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "inactive", "inactive@test.com");

            // user1 quiz tamamladı, user2 tamamlamadı
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id);

            // Act
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: user1.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal(user1.Id, result[0].UserId);
        }

        [Fact]
        public async Task GetGlobalLeaderboardAsync_OrdersByScoreDescending()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            // user2 daha fazla quiz tamamladı
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id);

            // Act
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: user1.Id);

            // Assert
            Assert.Equal(user2.Id, result[0].UserId);
            Assert.Equal(user1.Id, result[1].UserId);
        }

        [Fact]
        public async Task GetGlobalLeaderboardAsync_SameScore_OrdersBySuccessRateDescending()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            // İkisi de 1 quiz — user2'nin başarı oranı daha yüksek
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id, correctAnswers: 5);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id, correctAnswers: 9);

            // Act
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: user1.Id);

            // Assert
            Assert.Equal(user2.Id, result[0].UserId);
            Assert.Equal(user1.Id, result[1].UserId);
        }

        [Fact]
        public async Task GetGlobalLeaderboardAsync_AssignsRanksCorrectly()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");
            var user3 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user3", "u3@test.com");

            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user3.Id, category.Id);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user3.Id, category.Id);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user3.Id, category.Id);

            // Act
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: user1.Id);

            // Assert
            Assert.Equal(1, result[0].Rank);
            Assert.Equal(2, result[1].Rank);
            Assert.Equal(3, result[2].Rank);
        }

        [Fact]
        public async Task GetGlobalLeaderboardAsync_MarksCurrentUserCorrectly()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var currentUser = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "me", "me@test.com");
            var otherUser = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "other", "other@test.com");

            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, currentUser.Id, category.Id);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, otherUser.Id, category.Id);

            // Act
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: currentUser.Id);

            // Assert
            Assert.True(result.First(r => r.UserId == currentUser.Id).IsCurrentUser);
            Assert.False(result.First(r => r.UserId == otherUser.Id).IsCurrentUser);
        }

        [Fact]
        public async Task GetGlobalLeaderboardAsync_RespectsTopLimit()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            for (int i = 0; i < 10; i++)
            {
                var user = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, $"user{i}", $"u{i}@test.com");
                await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user.Id, category.Id);
            }

            // Act — top 5 iste
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: 1, top: 5);

            // Assert
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task GetGlobalLeaderboardAsync_CalculatesSuccessRateCorrectly()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user", "u@test.com");
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user.Id, category.Id,
                totalQuestions: 10, correctAnswers: 8);

            // Act
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: user.Id);

            // Assert
            Assert.Equal(80.0, result[0].SuccessRate);
        }

        [Fact]
        public async Task GetGlobalLeaderboardAsync_IncludesBadgeAndStreakCounts()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user", "u@test.com");
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user.Id, category.Id);

            var badge = await LeaderboardServiceTestHelpers.SeedBadgeAsync(_context);
            await LeaderboardServiceTestHelpers.SeedUserBadgeAsync(_context, user.Id, badge.Id);
            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, user.Id, currentStreak: 7);

            // Act
            var result = await _sut.GetGlobalLeaderboardAsync(currentUserId: user.Id);

            // Assert
            Assert.Equal(1, result[0].BadgeCount);
            Assert.Equal(7, result[0].CurrentStreak);
        }

        #endregion

        #region GetWeeklyLeaderboardAsync

        [Fact]
        public async Task GetWeeklyLeaderboardAsync_NoQuizzesThisWeek_ReturnsEmptyList()
        {
            // Arrange — quiz 8 gün önce tamamlanmış, haftalık dışında kalmalı
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user", "u@test.com");
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user.Id, category.Id, completedDaysAgo: 8);

            // Act
            var result = await _sut.GetWeeklyLeaderboardAsync(currentUserId: user.Id);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWeeklyLeaderboardAsync_OnlyCountsQuizzesFromLastWeek()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            // user1: 1 bu hafta + 2 geçen hafta (sadece 1 sayılmalı)
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id, completedDaysAgo: 3);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id, completedDaysAgo: 10);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id, completedDaysAgo: 14);

            // user2: 2 bu hafta
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id, completedDaysAgo: 1);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id, completedDaysAgo: 2);

            // Act
            var result = await _sut.GetWeeklyLeaderboardAsync(currentUserId: user1.Id);

            // Assert — user2 daha fazla haftalık quiz
            Assert.Equal(user2.Id, result[0].UserId);
            Assert.Equal(2, result[0].Score);
            Assert.Equal(user1.Id, result[1].UserId);
            Assert.Equal(1, result[1].Score);
        }

        [Fact]
        public async Task GetWeeklyLeaderboardAsync_AssignsRanksCorrectly()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id, completedDaysAgo: 1);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id, completedDaysAgo: 1);
            await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id, completedDaysAgo: 2);

            // Act
            var result = await _sut.GetWeeklyLeaderboardAsync(currentUserId: user1.Id);

            // Assert
            Assert.Equal(1, result[0].Rank);
            Assert.Equal(2, result[1].Rank);
        }

        #endregion

        #region GetStreakLeaderboardAsync

        [Fact]
        public async Task GetStreakLeaderboardAsync_NoStreaks_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetStreakLeaderboardAsync(currentUserId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetStreakLeaderboardAsync_ExcludesZeroStreaks()
        {
            // Arrange
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "active", "active@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "inactive", "inactive@test.com");

            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, user1.Id, currentStreak: 5);
            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, user2.Id, currentStreak: 0); // dahil edilmemeli

            // Act
            var result = await _sut.GetStreakLeaderboardAsync(currentUserId: user1.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal(user1.Id, result[0].UserId);
        }

        [Fact]
        public async Task GetStreakLeaderboardAsync_OrdersByCurrentStreakDescending()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, user1.Id, currentStreak: 3);
            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, user2.Id, currentStreak: 10);

            // Act
            var result = await _sut.GetStreakLeaderboardAsync(currentUserId: user1.Id);

            // Assert
            Assert.Equal(user2.Id, result[0].UserId);
            Assert.Equal(10, result[0].Score);
            Assert.Equal(user1.Id, result[1].UserId);
        }

        [Fact]
        public async Task GetStreakLeaderboardAsync_AssignsRanksCorrectly()
        {
            // Arrange
            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, user1.Id, currentStreak: 10);
            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, user2.Id, currentStreak: 5);

            // Act
            var result = await _sut.GetStreakLeaderboardAsync(currentUserId: user1.Id);

            // Assert
            Assert.Equal(1, result[0].Rank);
            Assert.Equal(2, result[1].Rank);
        }

        [Fact]
        public async Task GetStreakLeaderboardAsync_MarksCurrentUserCorrectly()
        {
            // Arrange
            var currentUser = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "me", "me@test.com");
            var otherUser = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "other", "other@test.com");

            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, currentUser.Id, currentStreak: 5);
            await LeaderboardServiceTestHelpers.SeedUserStreakAsync(_context, otherUser.Id, currentStreak: 3);

            // Act
            var result = await _sut.GetStreakLeaderboardAsync(currentUserId: currentUser.Id);

            // Assert
            Assert.True(result.First(r => r.UserId == currentUser.Id).IsCurrentUser);
            Assert.False(result.First(r => r.UserId == otherUser.Id).IsCurrentUser);
        }

        #endregion

        #region GetCategoryLeaderboardAsync

        [Fact]
        public async Task GetCategoryLeaderboardAsync_CategoryNotFound_ReturnsEmptyDto()
        {
            // Act
            var result = await _sut.GetCategoryLeaderboardAsync(categoryId: 999, currentUserId: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.CategoryName);
            Assert.Empty(result.Entries);
        }

        [Fact]
        public async Task GetCategoryLeaderboardAsync_ReturnsCorrectCategoryName()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context, "Python");

            // Act
            var result = await _sut.GetCategoryLeaderboardAsync(category.Id, currentUserId: 1);

            // Assert
            Assert.Equal("Python", result.CategoryName);
        }

        [Fact]
        public async Task GetCategoryLeaderboardAsync_OrdersByCorrectAnswersDescending()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var question = await LeaderboardServiceTestHelpers.SeedQuestionAsync(_context, category.Id);

            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            var quiz1 = await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id);
            var quiz2 = await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id);

            // user2 daha fazla doğru
            await LeaderboardServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz1.Id, question.Id, isCorrect: false);
            await LeaderboardServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz2.Id, question.Id, isCorrect: true);

            // Act
            var result = await _sut.GetCategoryLeaderboardAsync(category.Id, currentUserId: user1.Id);

            // Assert
            Assert.Equal(user2.Id, result.Entries[0].UserId);
            Assert.Equal(user1.Id, result.Entries[1].UserId);
        }

        [Fact]
        public async Task GetCategoryLeaderboardAsync_AssignsRanksCorrectly()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var question = await LeaderboardServiceTestHelpers.SeedQuestionAsync(_context, category.Id);

            var user1 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            var quiz1 = await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user1.Id, category.Id);
            var quiz2 = await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user2.Id, category.Id);

            await LeaderboardServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz1.Id, question.Id, isCorrect: true);
            await LeaderboardServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz2.Id, question.Id, isCorrect: false);

            // Act
            var result = await _sut.GetCategoryLeaderboardAsync(category.Id, currentUserId: user1.Id);

            // Assert
            Assert.Equal(1, result.Entries[0].Rank);
            Assert.Equal(2, result.Entries[1].Rank);
        }

        [Fact]
        public async Task GetCategoryLeaderboardAsync_MarksCurrentUserCorrectly()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var question = await LeaderboardServiceTestHelpers.SeedQuestionAsync(_context, category.Id);

            var currentUser = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "me", "me@test.com");
            var otherUser = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, "other", "other@test.com");

            var quiz1 = await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, currentUser.Id, category.Id);
            var quiz2 = await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, otherUser.Id, category.Id);

            await LeaderboardServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz1.Id, question.Id, isCorrect: true);
            await LeaderboardServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz2.Id, question.Id, isCorrect: true);

            // Act
            var result = await _sut.GetCategoryLeaderboardAsync(category.Id, currentUserId: currentUser.Id);

            // Assert
            Assert.True(result.Entries.First(e => e.UserId == currentUser.Id).IsCurrentUser);
            Assert.False(result.Entries.First(e => e.UserId == otherUser.Id).IsCurrentUser);
        }

        #endregion

        #region GetAllCategoryLeaderboardsAsync

        [Fact]
        public async Task GetAllCategoryLeaderboardsAsync_NoCategories_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetAllCategoryLeaderboardsAsync(currentUserId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCategoryLeaderboardsAsync_ReturnsOneEntryPerCategory()
        {
            // Arrange
            await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context, "C#");
            await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context, "Python");
            await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context, "Java");

            // Act
            var result = await _sut.GetAllCategoryLeaderboardsAsync(currentUserId: 1);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, r => r.CategoryName == "C#");
            Assert.Contains(result, r => r.CategoryName == "Python");
            Assert.Contains(result, r => r.CategoryName == "Java");
        }

        [Fact]
        public async Task GetAllCategoryLeaderboardsAsync_EachLeaderboardLimitedToTop10()
        {
            // Arrange
            var category = await LeaderboardServiceTestHelpers.SeedCategoryAsync(_context);
            var question = await LeaderboardServiceTestHelpers.SeedQuestionAsync(_context, category.Id);

            for (int i = 0; i < 15; i++)
            {
                var user = await LeaderboardServiceTestHelpers.SeedUserAsync(_context, $"user{i}", $"u{i}@test.com");
                var quiz = await LeaderboardServiceTestHelpers.SeedCompletedQuizAsync(_context, user.Id, category.Id);
                await LeaderboardServiceTestHelpers.SeedQuizQuestionAsync(_context, quiz.Id, question.Id, isCorrect: true);
            }

            // Act
            var result = await _sut.GetAllCategoryLeaderboardsAsync(currentUserId: 1);

            // Assert — GetCategoryLeaderboardAsync top:10 ile çağrılıyor
            Assert.Equal(10, result[0].Entries.Count);
        }

        #endregion
    }
}