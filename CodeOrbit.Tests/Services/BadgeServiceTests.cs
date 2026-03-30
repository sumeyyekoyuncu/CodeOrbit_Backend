using CodeOrbit.Application.DTOs.Notification;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;
using CodeOrbit.Infrastructure.Services;
using CodeOrbit.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CodeOrbit.Tests.Services
{
    public class BadgeServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly BadgeService _sut;

        public BadgeServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _notificationMock = new Mock<INotificationService>();
            _sut = new BadgeService(_context, _notificationMock.Object);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region GetUserBadgesAsync

        [Fact]
        public async Task GetUserBadgesAsync_NoBadgesInSystem_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserBadgesAsync_UserHasNotEarnedBadge_IsEarnedFalse()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "İlk Quiz", "Complete1Quiz", 1);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Single(result);
            Assert.False(result[0].IsEarned);
            Assert.Null(result[0].EarnedAt);
        }

        [Fact]
        public async Task GetUserBadgesAsync_UserHasEarnedBadge_IsEarnedTrueWithDate()
        {
            // Arrange
            var badge = await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "İlk Quiz", "Complete1Quiz", 1);
            await BadgeServiceTestHelpers.SeedEarnedBadgeAsync(_context, userId: 1, badgeId: badge.Id);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.True(result[0].IsEarned);
            Assert.NotNull(result[0].EarnedAt);
        }

        [Fact]
        public async Task GetUserBadgesAsync_ReturnsAllBadgesRegardlessOfEarned()
        {
            // Arrange — 3 rozet ekle, sadece 1 tanesi kazanılmış
            var b1 = await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "Rozet 1", "Complete1Quiz", 1);
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "Rozet 2", "Complete10Quizzes", 10);
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "Rozet 3", "Complete50Quizzes", 50);
            await BadgeServiceTestHelpers.SeedEarnedBadgeAsync(_context, userId: 1, badgeId: b1.Id);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result.Count(r => r.IsEarned));
        }

        [Fact]
        public async Task GetUserBadgesAsync_ReturnsCorrectRequiredCount()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "10 Quiz", "Complete10Quizzes", 10);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Equal(10, result[0].RequiredCount);
        }

        #endregion

        #region GetUserBadgesAsync — Progress Hesaplama

        [Fact]
        public async Task GetUserBadgesAsync_CompleteQuizRequirement_ReturnsCorrectProgress()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "10 Quiz", "Complete10Quizzes", 10);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 4);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Equal(4, result[0].Progress);
        }

        [Fact]
        public async Task GetUserBadgesAsync_PerfectScoreRequirement_CountsOnlyPerfectQuizzes()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "Mükemmel", "PerfectScore", 1);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 1, perfect: true);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 2, perfect: false);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert — sadece 1 tanesi perfect
            Assert.Equal(1, result[0].Progress);
        }

        [Fact]
        public async Task GetUserBadgesAsync_Streak7DaysRequirement_ReturnsCurrentStreak()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "7 Gün Seri", "Streak7Days", 7);
            await BadgeServiceTestHelpers.SeedUserStreakAsync(_context, userId: 1, currentStreak: 5, longestStreak: 10);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Equal(5, result[0].Progress);
        }

        [Fact]
        public async Task GetUserBadgesAsync_Streak30DaysRequirement_ReturnsLongestStreak()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "30 Gün Seri", "Streak30Days", 30);
            await BadgeServiceTestHelpers.SeedUserStreakAsync(_context, userId: 1, currentStreak: 5, longestStreak: 35);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Equal(35, result[0].Progress);
        }

        [Fact]
        public async Task GetUserBadgesAsync_Have5FriendsRequirement_CountsFriendships()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "Sosyal", "Have5Friends", 5);
            await BadgeServiceTestHelpers.SeedFriendshipsAsync(_context, userId: 1, friendCount: 3);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Equal(3, result[0].Progress);
        }

        [Fact]
        public async Task GetUserBadgesAsync_UnknownRequirement_ReturnsZeroProgress()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "Bilinmeyen", "UnknownRequirement", 5);

            // Act
            var result = await _sut.GetUserBadgesAsync(userId: 1);

            // Assert
            Assert.Equal(0, result[0].Progress);
        }

        #endregion

        #region CheckAndAwardBadgesAsync

        [Fact]
        public async Task CheckAndAwardBadgesAsync_NoBadgesInSystem_DoesNothing()
        {
            // Act
            await _sut.CheckAndAwardBadgesAsync(userId: 1);

            // Assert
            Assert.Empty(await _context.UserBadges.ToListAsync());
        }

        [Fact]
        public async Task CheckAndAwardBadgesAsync_ConditionMet_AwardsBadge()
        {
            // Arrange
            var badge = await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "İlk Quiz", "Complete1Quiz", 1);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 1);

            // Act
            await _sut.CheckAndAwardBadgesAsync(userId: 1);

            // Assert
            var userBadge = await _context.UserBadges
                .FirstOrDefaultAsync(ub => ub.UserId == 1 && ub.BadgeId == badge.Id);
            Assert.NotNull(userBadge);
        }

        [Fact]
        public async Task CheckAndAwardBadgesAsync_ConditionNotMet_DoesNotAwardBadge()
        {
            // Arrange — 10 quiz gerekiyor, 3 tamamlanmış
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "10 Quiz", "Complete10Quizzes", 10);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 3);

            // Act
            await _sut.CheckAndAwardBadgesAsync(userId: 1);

            // Assert
            Assert.Empty(await _context.UserBadges.ToListAsync());
        }

        [Fact]
        public async Task CheckAndAwardBadgesAsync_AlreadyEarned_DoesNotDuplicateBadge()
        {
            // Arrange
            var badge = await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "İlk Quiz", "Complete1Quiz", 1);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 5);
            await BadgeServiceTestHelpers.SeedEarnedBadgeAsync(_context, userId: 1, badgeId: badge.Id);

            // Act
            await _sut.CheckAndAwardBadgesAsync(userId: 1);

            // Assert — hâlâ 1 tane olmalı
            var count = await _context.UserBadges.CountAsync(ub => ub.UserId == 1 && ub.BadgeId == badge.Id);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CheckAndAwardBadgesAsync_ConditionMet_CallsCreateNotificationOnce()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "İlk Quiz", "Complete1Quiz", 1);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 1);

            // Act
            await _sut.CheckAndAwardBadgesAsync(userId: 1);

            // Assert — mock sayesinde doğrudan doğrulayabiliyoruz
            _notificationMock.Verify(
                n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CheckAndAwardBadgesAsync_ConditionNotMet_DoesNotCallCreateNotification()
        {
            // Arrange
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "10 Quiz", "Complete10Quizzes", 10);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 3);

            // Act
            await _sut.CheckAndAwardBadgesAsync(userId: 1);

            // Assert
            _notificationMock.Verify(
                n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()),
                Times.Never
            );
        }

        [Fact]
        public async Task CheckAndAwardBadgesAsync_MultipleBadgesConditionsMet_AwardsAllAndNotifiesForEach()
        {
            // Arrange — hem 1 hem 10 quiz rozetini kazanacak kadar quiz
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "İlk Quiz", "Complete1Quiz", 1);
            await BadgeServiceTestHelpers.SeedBadgeAsync(_context, "10 Quiz", "Complete10Quizzes", 10);
            await BadgeServiceTestHelpers.SeedCompletedQuizzesAsync(_context, userId: 1, count: 10);

            // Act
            await _sut.CheckAndAwardBadgesAsync(userId: 1);

            // Assert
            var earned = await _context.UserBadges.Where(ub => ub.UserId == 1).ToListAsync();
            Assert.Equal(2, earned.Count);

            _notificationMock.Verify(
                n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()),
                Times.Exactly(2)
            );
        }

        #endregion
    }
}