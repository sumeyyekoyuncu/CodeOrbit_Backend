using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using CodeOrbit.Infrastructure.Services;
using CodeOrbit.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CodeOrbit.Tests.Services
{
    public class NotificationServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly NotificationService _sut;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new NotificationService(_context);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region CreateNotificationAsync

        [Fact]
        public async Task CreateNotificationAsync_ValidDto_PersistsNotification()
        {
            // Arrange
            var dto = NotificationServiceTestHelpers.BuildCreateDto(userId: 1);

            // Act
            await _sut.CreateNotificationAsync(dto);

            // Assert
            var saved = await _context.Notifications.FirstOrDefaultAsync(n => n.UserId == 1);
            Assert.NotNull(saved);
            Assert.Equal(dto.Title, saved.Title);
            Assert.Equal(dto.Message, saved.Message);
            Assert.Equal(dto.Type, saved.Type);
            Assert.Equal(dto.ActionUrl, saved.ActionUrl);
        }

        [Fact]
        public async Task CreateNotificationAsync_NewNotification_IsReadFalseByDefault()
        {
            // Arrange
            var dto = NotificationServiceTestHelpers.BuildCreateDto(userId: 1);

            // Act
            await _sut.CreateNotificationAsync(dto);

            // Assert
            var saved = await _context.Notifications.FirstOrDefaultAsync();
            Assert.False(saved!.IsRead);
        }

        [Fact]
        public async Task CreateNotificationAsync_NewNotification_SetsCreatedAt()
        {
            // Arrange
            var before = DateTime.UtcNow.AddSeconds(-1);
            var dto = NotificationServiceTestHelpers.BuildCreateDto(userId: 1);

            // Act
            await _sut.CreateNotificationAsync(dto);

            // Assert
            var saved = await _context.Notifications.FirstOrDefaultAsync();
            Assert.True(saved!.CreatedAt >= before && saved.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
        }

        #endregion

        #region GetUserNotificationsAsync

        [Fact]
        public async Task GetUserNotificationsAsync_NoNotifications_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetUserNotificationsAsync(userId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ReturnsOnlyCurrentUsersNotifications()
        {
            // Arrange
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1);
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 2); // başka user

            // Act
            var result = await _sut.GetUserNotificationsAsync(userId: 1);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_UnreadOnlyFalse_ReturnsBothReadAndUnread()
        {
            // Arrange
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: false);
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: true);

            // Act
            var result = await _sut.GetUserNotificationsAsync(userId: 1, unreadOnly: false);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_UnreadOnlyTrue_ReturnsOnlyUnread()
        {
            // Arrange
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: false);
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: true);

            // Act
            var result = await _sut.GetUserNotificationsAsync(userId: 1, unreadOnly: true);

            // Assert
            Assert.Single(result);
            Assert.False(result[0].IsRead);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ReturnsOrderedByCreatedAtDescending()
        {
            // Arrange
            var older = await NotificationServiceTestHelpers.SeedNotificationAsync(
                _context, userId: 1, createdAt: DateTime.UtcNow.AddDays(-2));
            var newer = await NotificationServiceTestHelpers.SeedNotificationAsync(
                _context, userId: 1, createdAt: DateTime.UtcNow.AddDays(-1));

            // Act
            var result = await _sut.GetUserNotificationsAsync(userId: 1);

            // Assert — en yeni önce gelmeli
            Assert.Equal(newer.Id, result[0].Id);
            Assert.Equal(older.Id, result[1].Id);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ReturnsMappedDtoFieldsCorrectly()
        {
            // Arrange
            var notification = await NotificationServiceTestHelpers.SeedNotificationAsync(
                _context, userId: 1, type: NotificationType.DailyChallenge);

            // Act
            var result = await _sut.GetUserNotificationsAsync(userId: 1);

            // Assert
            var dto = result[0];
            Assert.Equal(notification.Id, dto.Id);
            Assert.Equal("DailyChallenge", dto.Type);
            Assert.Equal(notification.Title, dto.Title);
            Assert.Equal(notification.Message, dto.Message);
            Assert.Equal(notification.IsRead, dto.IsRead);
        }

        #endregion

        #region GetUnreadCountAsync

        [Fact]
        public async Task GetUnreadCountAsync_NoNotifications_ReturnsZero()
        {
            // Act
            var result = await _sut.GetUnreadCountAsync(userId: 1);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetUnreadCountAsync_CountsOnlyUnreadForCurrentUser()
        {
            // Arrange
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: false);
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: false);
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: true);  // okunmuş, sayılmamalı
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 2, isRead: false); // başka user, sayılmamalı

            // Act
            var result = await _sut.GetUnreadCountAsync(userId: 1);

            // Assert
            Assert.Equal(2, result);
        }

        #endregion

        #region MarkAsReadAsync

        [Fact]
        public async Task MarkAsReadAsync_ExistingNotification_ReturnsTrueAndMarksRead()
        {
            // Arrange
            var notification = await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: false);

            // Act
            var result = await _sut.MarkAsReadAsync(notification.Id);

            // Assert
            Assert.True(result);
            var updated = await _context.Notifications.FindAsync(notification.Id);
            Assert.True(updated!.IsRead);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotificationNotFound_ReturnsFalse()
        {
            // Act
            var result = await _sut.MarkAsReadAsync(notificationId: 999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region MarkAllAsReadAsync

        [Fact]
        public async Task MarkAllAsReadAsync_MarksAllUnreadForUser()
        {
            // Arrange
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: false);
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: false);
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: true);

            // Act
            var result = await _sut.MarkAllAsReadAsync(userId: 1);

            // Assert
            Assert.True(result);
            var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == 1 && !n.IsRead);
            Assert.Equal(0, unreadCount);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_DoesNotAffectOtherUsersNotifications()
        {
            // Arrange
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: false);
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 2, isRead: false); // başka user

            // Act
            await _sut.MarkAllAsReadAsync(userId: 1);

            // Assert — user2'nin bildirimi etkilenmemeli
            var user2Unread = await _context.Notifications.CountAsync(n => n.UserId == 2 && !n.IsRead);
            Assert.Equal(1, user2Unread);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_NoUnreadNotifications_StillReturnsTrue()
        {
            // Arrange — zaten okunmuş
            await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1, isRead: true);

            // Act
            var result = await _sut.MarkAllAsReadAsync(userId: 1);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region DeleteNotificationAsync

        [Fact]
        public async Task DeleteNotificationAsync_ExistingNotification_ReturnsTrueAndDeletes()
        {
            // Arrange
            var notification = await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1);

            // Act
            var result = await _sut.DeleteNotificationAsync(notification.Id);

            // Assert
            Assert.True(result);
            var deleted = await _context.Notifications.FindAsync(notification.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteNotificationAsync_NotificationNotFound_ReturnsFalse()
        {
            // Act
            var result = await _sut.DeleteNotificationAsync(notificationId: 999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteNotificationAsync_OnlyDeletesTargetNotification()
        {
            // Arrange
            var target = await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1);
            var other = await NotificationServiceTestHelpers.SeedNotificationAsync(_context, userId: 1);

            // Act
            await _sut.DeleteNotificationAsync(target.Id);

            // Assert
            Assert.Null(await _context.Notifications.FindAsync(target.Id));
            Assert.NotNull(await _context.Notifications.FindAsync(other.Id));
        }

        #endregion

        #region CheckAndSendStreakWarningsAsync

        [Fact]
        public async Task CheckAndSendStreakWarningsAsync_UserActiveYesterdayNoActivityToday_SendsWarning()
        {
            // Arrange
            var user = await NotificationServiceTestHelpers.SeedUserAsync(_context);
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            await NotificationServiceTestHelpers.SeedUserStreakAsync(_context, user.Id, currentStreak: 5, lastActiveDate: yesterday);

            // Act
            await _sut.CheckAndSendStreakWarningsAsync();

            // Assert
            var warning = await _context.Notifications.FirstOrDefaultAsync(n =>
                n.UserId == user.Id && n.Type == NotificationType.StreakWarning);
            Assert.NotNull(warning);
        }

        [Fact]
        public async Task CheckAndSendStreakWarningsAsync_UserHasActivityToday_DoesNotSendWarning()
        {
            // Arrange
            var user = await NotificationServiceTestHelpers.SeedUserAsync(_context);
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            await NotificationServiceTestHelpers.SeedUserStreakAsync(_context, user.Id, currentStreak: 5, lastActiveDate: yesterday);
            await NotificationServiceTestHelpers.SeedUserActivityAsync(_context, user.Id, DateTime.UtcNow.Date);

            // Act
            await _sut.CheckAndSendStreakWarningsAsync();

            // Assert
            var warning = await _context.Notifications.FirstOrDefaultAsync(n =>
                n.UserId == user.Id && n.Type == NotificationType.StreakWarning);
            Assert.Null(warning);
        }

        [Fact]
        public async Task CheckAndSendStreakWarningsAsync_AlreadyWarnedToday_DoesNotSendDuplicate()
        {
            // Arrange
            var user = await NotificationServiceTestHelpers.SeedUserAsync(_context);
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            await NotificationServiceTestHelpers.SeedUserStreakAsync(_context, user.Id, currentStreak: 5, lastActiveDate: yesterday);

            // Bugün zaten uyarı gönderilmiş
            await NotificationServiceTestHelpers.SeedNotificationAsync(
                _context, user.Id, type: NotificationType.StreakWarning, createdAt: DateTime.UtcNow);

            // Act
            await _sut.CheckAndSendStreakWarningsAsync();

            // Assert — hâlâ 1 tane olmalı
            var count = await _context.Notifications.CountAsync(n =>
                n.UserId == user.Id && n.Type == NotificationType.StreakWarning);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CheckAndSendStreakWarningsAsync_ZeroStreak_DoesNotSendWarning()
        {
            // Arrange — streak 0, uyarı gönderilmemeli
            var user = await NotificationServiceTestHelpers.SeedUserAsync(_context);
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            await NotificationServiceTestHelpers.SeedUserStreakAsync(_context, user.Id, currentStreak: 0, lastActiveDate: yesterday);

            // Act
            await _sut.CheckAndSendStreakWarningsAsync();

            // Assert
            Assert.Empty(await _context.Notifications.ToListAsync());
        }

        [Fact]
        public async Task CheckAndSendStreakWarningsAsync_LastActiveTwoDaysAgo_DoesNotSendWarning()
        {
            // Arrange — dün değil, 2 gün önce aktif olmuş (streak zaten kırılmış)
            var user = await NotificationServiceTestHelpers.SeedUserAsync(_context);
            var twoDaysAgo = DateTime.UtcNow.Date.AddDays(-2);
            await NotificationServiceTestHelpers.SeedUserStreakAsync(_context, user.Id, currentStreak: 5, lastActiveDate: twoDaysAgo);

            // Act
            await _sut.CheckAndSendStreakWarningsAsync();

            // Assert — servis sadece dünü kontrol ediyor
            Assert.Empty(await _context.Notifications.ToListAsync());
        }

        #endregion
    }
}