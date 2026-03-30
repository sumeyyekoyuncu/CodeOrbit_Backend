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
    public class FriendServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly FriendService _sut;

        public FriendServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _notificationMock = new Mock<INotificationService>();
            _sut = new FriendService(_context, _notificationMock.Object);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region SendFriendRequestAsync

        [Fact]
        public async Task SendFriendRequestAsync_ValidRequest_ReturnsTrueAndPersists()
        {
            // Arrange
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var dto = FriendServiceTestHelpers.BuildSendRequestDto(sender.Id, receiver.Id);

            // Act
            var result = await _sut.SendFriendRequestAsync(dto);

            // Assert
            Assert.True(result);
            var saved = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == sender.Id && fr.ReceiverId == receiver.Id);
            Assert.NotNull(saved);
            Assert.Equal(FriendRequestStatus.Pending, saved.Status);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ValidRequest_SendsNotificationToReceiver()
        {
            // Arrange
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var dto = FriendServiceTestHelpers.BuildSendRequestDto(sender.Id, receiver.Id);

            // Act
            await _sut.SendFriendRequestAsync(dto);

            // Assert — receiver'a bildirim gitmeli
            _notificationMock.Verify(n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.UserId == receiver.Id &&
                    d.Type == NotificationType.FriendRequest)),
                Times.Once);
        }

        [Fact]
        public async Task SendFriendRequestAsync_SenderEqualsReceiver_ReturnsFalse()
        {
            // Arrange
            var user = await FriendServiceTestHelpers.SeedUserAsync(_context, "user", "user@test.com");
            var dto = FriendServiceTestHelpers.BuildSendRequestDto(user.Id, user.Id);

            // Act
            var result = await _sut.SendFriendRequestAsync(dto);

            // Assert
            Assert.False(result);
            _notificationMock.Verify(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task SendFriendRequestAsync_AlreadyFriends_ReturnsFalse()
        {
            // Arrange
            var user1 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");
            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, user1.Id, user2.Id);
            var dto = FriendServiceTestHelpers.BuildSendRequestDto(user1.Id, user2.Id);

            // Act
            var result = await _sut.SendFriendRequestAsync(dto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendFriendRequestAsync_PendingRequestExists_ReturnsFalse()
        {
            // Arrange
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender.Id, receiver.Id, FriendRequestStatus.Pending);
            var dto = FriendServiceTestHelpers.BuildSendRequestDto(sender.Id, receiver.Id);

            // Act
            var result = await _sut.SendFriendRequestAsync(dto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ReversePendingRequestExists_ReturnsFalse()
        {
            // Arrange — receiver daha önce sender'a istek atmış
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, receiver.Id, sender.Id, FriendRequestStatus.Pending);
            var dto = FriendServiceTestHelpers.BuildSendRequestDto(sender.Id, receiver.Id);

            // Act
            var result = await _sut.SendFriendRequestAsync(dto);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region RespondToRequestAsync

        [Fact]
        public async Task RespondToRequestAsync_Accept_ReturnsTrueAndCreatesFriendship()
        {
            // Arrange
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var request = await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender.Id, receiver.Id);
            var dto = FriendServiceTestHelpers.BuildRespondDto(request.Id, accept: true);

            // Act
            var result = await _sut.RespondToRequestAsync(receiver.Id, dto);

            // Assert
            Assert.True(result);
            var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
                (f.User1Id == sender.Id && f.User2Id == receiver.Id) ||
                (f.User1Id == receiver.Id && f.User2Id == sender.Id));
            Assert.NotNull(friendship);
        }

        [Fact]
        public async Task RespondToRequestAsync_Accept_UpdatesRequestStatusToAccepted()
        {
            // Arrange
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var request = await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender.Id, receiver.Id);
            var dto = FriendServiceTestHelpers.BuildRespondDto(request.Id, accept: true);

            // Act
            await _sut.RespondToRequestAsync(receiver.Id, dto);

            // Assert
            var updated = await _context.FriendRequests.FindAsync(request.Id);
            Assert.Equal(FriendRequestStatus.Accepted, updated!.Status);
            Assert.NotNull(updated.RespondedAt);
        }

        [Fact]
        public async Task RespondToRequestAsync_Accept_NotifiesSenderWithAcceptedType()
        {
            // Arrange
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var request = await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender.Id, receiver.Id);
            var dto = FriendServiceTestHelpers.BuildRespondDto(request.Id, accept: true);

            // Act
            await _sut.RespondToRequestAsync(receiver.Id, dto);

            // Assert
            _notificationMock.Verify(n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.UserId == sender.Id &&
                    d.Type == NotificationType.FriendRequestAccepted)),
                Times.Once);
        }

        [Fact]
        public async Task RespondToRequestAsync_Reject_ReturnsTrueAndDoesNotCreateFriendship()
        {
            // Arrange
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var request = await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender.Id, receiver.Id);
            var dto = FriendServiceTestHelpers.BuildRespondDto(request.Id, accept: false);

            // Act
            var result = await _sut.RespondToRequestAsync(receiver.Id, dto);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.Friendships.ToListAsync());
        }

        [Fact]
        public async Task RespondToRequestAsync_Reject_NotifiesSenderWithRejectedType()
        {
            // Arrange
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var request = await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender.Id, receiver.Id);
            var dto = FriendServiceTestHelpers.BuildRespondDto(request.Id, accept: false);

            // Act
            await _sut.RespondToRequestAsync(receiver.Id, dto);

            // Assert
            _notificationMock.Verify(n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.UserId == sender.Id &&
                    d.Type == NotificationType.FriendRequestRejected)),
                Times.Once);
        }

        [Fact]
        public async Task RespondToRequestAsync_WrongReceiver_ReturnsFalse()
        {
            // Arrange — yanlış userId ile yanıt vermeye çalışıyor
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var request = await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender.Id, receiver.Id);
            var dto = FriendServiceTestHelpers.BuildRespondDto(request.Id, accept: true);

            // Act — sender kendi isteğini kabul etmeye çalışıyor
            var result = await _sut.RespondToRequestAsync(sender.Id, dto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RespondToRequestAsync_AlreadyResponded_ReturnsFalse()
        {
            // Arrange — zaten kabul edilmiş istek
            var sender = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender", "sender@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "receiver@test.com");
            var request = await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender.Id, receiver.Id, FriendRequestStatus.Accepted);
            var dto = FriendServiceTestHelpers.BuildRespondDto(request.Id, accept: true);

            // Act
            var result = await _sut.RespondToRequestAsync(receiver.Id, dto);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetPendingRequestsAsync

        [Fact]
        public async Task GetPendingRequestsAsync_NoPendingRequests_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetPendingRequestsAsync(userId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPendingRequestsAsync_ReturnsOnlyPendingForCurrentUser()
        {
            // Arrange
            var sender1 = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender1", "s1@test.com");
            var sender2 = await FriendServiceTestHelpers.SeedUserAsync(_context, "sender2", "s2@test.com");
            var receiver = await FriendServiceTestHelpers.SeedUserAsync(_context, "receiver", "r@test.com");
            var otherUser = await FriendServiceTestHelpers.SeedUserAsync(_context, "other", "o@test.com");

            await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender1.Id, receiver.Id); // pending
            await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender2.Id, receiver.Id); // pending
            await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender1.Id, otherUser.Id); // başka user'ın isteği
            await FriendServiceTestHelpers.SeedFriendRequestAsync(_context, sender2.Id, receiver.Id, FriendRequestStatus.Accepted); // accepted, gelmemeli

            // Act
            var result = await _sut.GetPendingRequestsAsync(receiver.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("Pending", r.Status));
        }

        #endregion

        #region GetFriendsAsync

        [Fact]
        public async Task GetFriendsAsync_NoFriends_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetFriendsAsync(userId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFriendsAsync_ReturnsFriendFromBothUserPerspectives()
        {
            // Arrange
            var user1 = await FriendServiceTestHelpers.SeedUserAsync(_context, "alice", "alice@test.com");
            var user2 = await FriendServiceTestHelpers.SeedUserAsync(_context, "bob", "bob@test.com");
            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, user1.Id, user2.Id);

            // Act
            var fromUser1 = await _sut.GetFriendsAsync(user1.Id);
            var fromUser2 = await _sut.GetFriendsAsync(user2.Id);

            // Assert — her iki yönden de görünmeli
            Assert.Single(fromUser1);
            Assert.Equal(user2.Id, fromUser1[0].UserId);

            Assert.Single(fromUser2);
            Assert.Equal(user1.Id, fromUser2[0].UserId);
        }

        [Fact]
        public async Task GetFriendsAsync_ReturnsCorrectStats()
        {
            // Arrange
            var user1 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");
            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, user1.Id, user2.Id);

            // user2'nin quiz ve streak verileri
            _context.Quizzes.Add(new Domain.Entities.Quiz
            {
                UserId = user2.Id,
                CompletedAt = DateTime.UtcNow,
                TotalQuestions = 10,
                CorrectAnswers = 8,
                CategoryId = 1,
                DifficultyLevel = DifficultyLevel.Easy
            });
            _context.UserStreaks.Add(new Domain.Entities.UserStreak
            {
                UserId = user2.Id,
                CurrentStreak = 5,
                LongestStreak = 10,
                LastActiveDate = DateTime.UtcNow.Date
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetFriendsAsync(user1.Id);

            // Assert
            var stats = result[0].Stats;
            Assert.Equal(1, stats.TotalQuizzes);
            Assert.Equal(5, stats.CurrentStreak);
            Assert.Equal(80.0, stats.OverallSuccessRate);
        }

        [Fact]
        public async Task GetFriendsAsync_ReturnsOrderedByUsername()
        {
            // Arrange
            var user = await FriendServiceTestHelpers.SeedUserAsync(_context, "user", "u@test.com");
            var charlie = await FriendServiceTestHelpers.SeedUserAsync(_context, "charlie", "c@test.com");
            var alice = await FriendServiceTestHelpers.SeedUserAsync(_context, "alice", "a@test.com");
            var bob = await FriendServiceTestHelpers.SeedUserAsync(_context, "bob", "b@test.com");

            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, user.Id, charlie.Id);
            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, user.Id, alice.Id);
            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, user.Id, bob.Id);

            // Act
            var result = await _sut.GetFriendsAsync(user.Id);

            // Assert
            Assert.Equal("alice", result[0].Username);
            Assert.Equal("bob", result[1].Username);
            Assert.Equal("charlie", result[2].Username);
        }

        #endregion

        #region RemoveFriendAsync

        [Fact]
        public async Task RemoveFriendAsync_ExistingFriendship_ReturnsTrueAndRemoves()
        {
            // Arrange
            var user1 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");
            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, user1.Id, user2.Id);

            // Act
            var result = await _sut.RemoveFriendAsync(user1.Id, user2.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.Friendships.ToListAsync());
        }

        [Fact]
        public async Task RemoveFriendAsync_NotFriends_ReturnsFalse()
        {
            // Arrange
            var user1 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");

            // Act
            var result = await _sut.RemoveFriendAsync(user1.Id, user2.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveFriendAsync_WorksFromBothDirections()
        {
            // Arrange — user2 silme isteği başlatıyor
            var user1 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user1", "u1@test.com");
            var user2 = await FriendServiceTestHelpers.SeedUserAsync(_context, "user2", "u2@test.com");
            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, user1.Id, user2.Id);

            // Act
            var result = await _sut.RemoveFriendAsync(user2.Id, user1.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.Friendships.ToListAsync());
        }

        #endregion

        #region SearchUsersAsync

        [Fact]
        public async Task SearchUsersAsync_MatchingUsername_ReturnsUser()
        {
            // Arrange
            var currentUser = await FriendServiceTestHelpers.SeedUserAsync(_context, "currentuser", "current@test.com");
            await FriendServiceTestHelpers.SeedUserAsync(_context, "john_doe", "john@test.com");

            // Act
            var result = await _sut.SearchUsersAsync("john", currentUser.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal("john_doe", result[0].Username);
        }

        [Fact]
        public async Task SearchUsersAsync_DoesNotReturnCurrentUser()
        {
            // Arrange
            var currentUser = await FriendServiceTestHelpers.SeedUserAsync(_context, "currentuser", "current@test.com");

            // Act
            var result = await _sut.SearchUsersAsync("currentuser", currentUser.Id);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchUsersAsync_DoesNotReturnAlreadyFriends()
        {
            // Arrange
            var currentUser = await FriendServiceTestHelpers.SeedUserAsync(_context, "currentuser", "current@test.com");
            var friend = await FriendServiceTestHelpers.SeedUserAsync(_context, "myfriend", "friend@test.com");
            await FriendServiceTestHelpers.SeedFriendshipAsync(_context, currentUser.Id, friend.Id);

            // Act
            var result = await _sut.SearchUsersAsync("myfriend", currentUser.Id);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchUsersAsync_MatchingEmail_ReturnsUser()
        {
            // Arrange
            var currentUser = await FriendServiceTestHelpers.SeedUserAsync(_context, "currentuser", "current@test.com");
            await FriendServiceTestHelpers.SeedUserAsync(_context, "janedoe", "jane@example.com");

            // Act
            var result = await _sut.SearchUsersAsync("example.com", currentUser.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal("janedoe", result[0].Username);
        }

        [Fact]
        public async Task SearchUsersAsync_NoMatch_ReturnsEmptyList()
        {
            // Arrange
            var currentUser = await FriendServiceTestHelpers.SeedUserAsync(_context, "currentuser", "current@test.com");

            // Act
            var result = await _sut.SearchUsersAsync("zzznomatch", currentUser.Id);

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}