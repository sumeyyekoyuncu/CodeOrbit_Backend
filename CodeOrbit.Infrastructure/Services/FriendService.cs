using CodeOrbit.Application.DTOs.Friend;
using CodeOrbit.Application.DTOs.Notification;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class FriendService : IFriendService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public FriendService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> SendFriendRequestAsync(SendFriendRequestDto dto)
        {
            if (dto.SenderId == dto.ReceiverId)
                return false;

            var alreadyFriends = await AreFriendsAsync(dto.SenderId, dto.ReceiverId);
            if (alreadyFriends)
                return false;

            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    ((fr.SenderId == dto.SenderId && fr.ReceiverId == dto.ReceiverId) ||
                     (fr.SenderId == dto.ReceiverId && fr.ReceiverId == dto.SenderId)) &&
                    fr.Status == FriendRequestStatus.Pending);

            if (existingRequest != null)
                return false;

            var request = new FriendRequest
            {
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                Status = FriendRequestStatus.Pending,
                SentAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(request);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(dto.SenderId);
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = dto.ReceiverId,
                Type = NotificationType.FriendRequest,
                Title = "Yeni arkadaşlık isteği",
                Message = $"{sender?.Username} size arkadaşlık isteği gönderdi",
                ActionUrl = "/friends/requests",
                RelatedEntityId = request.Id
            });

            return true;
        }

        public async Task<bool> RespondToRequestAsync(int userId, RespondToRequestDto dto)
        {
            var request = await _context.FriendRequests
                .Include(fr => fr.Receiver)
                .FirstOrDefaultAsync(fr => fr.Id == dto.RequestId && fr.ReceiverId == userId);

            if (request == null || request.Status != FriendRequestStatus.Pending)
                return false;

            request.Status = dto.Accept ? FriendRequestStatus.Accepted : FriendRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            if (dto.Accept)
            {
                var friendship = new Friendship
                {
                    User1Id = Math.Min(request.SenderId, request.ReceiverId),
                    User2Id = Math.Max(request.SenderId, request.ReceiverId),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Friendships.Add(friendship);
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = request.SenderId,
                Type = dto.Accept ? NotificationType.FriendRequestAccepted : NotificationType.FriendRequestRejected,
                Title = dto.Accept ? "Arkadaşlık isteği kabul edildi" : "Arkadaşlık isteği reddedildi",
                Message = dto.Accept
                    ? $"{request.Receiver.Username} arkadaşlık isteğinizi kabul etti"
                    : $"{request.Receiver.Username} arkadaşlık isteğinizi reddetti",
                ActionUrl = dto.Accept ? "/friends" : null
            });

            return true;
        }

        public async Task<List<FriendRequestDto>> GetPendingRequestsAsync(int userId)
        {
            return await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .Include(fr => fr.Sender)
                .OrderByDescending(fr => fr.SentAt)
                .Select(fr => new FriendRequestDto
                {
                    RequestId = fr.Id,
                    SenderId = fr.SenderId,
                    SenderUsername = fr.Sender.Username,
                    Status = fr.Status.ToString(),
                    SentAt = fr.SentAt
                })
                .ToListAsync();
        }

        public async Task<List<FriendDto>> GetFriendsAsync(int userId)
        {
            var friendships = await _context.Friendships
                .Where(f => f.User1Id == userId || f.User2Id == userId)
                .Include(f => f.User1)
                .Include(f => f.User2)
                .ToListAsync();

            var friendDtos = new List<FriendDto>();

            foreach (var friendship in friendships)
            {
                var friend = friendship.User1Id == userId ? friendship.User2 : friendship.User1;
                var friendId = friend.Id;

                var quizCount = await _context.Quizzes
                    .CountAsync(q => q.UserId == friendId && q.CompletedAt.HasValue);

                var streak = await _context.UserStreaks
                    .FirstOrDefaultAsync(s => s.UserId == friendId);

                var quizzes = await _context.Quizzes
                    .Where(q => q.UserId == friendId && q.CompletedAt.HasValue)
                    .ToListAsync();

                var successRate = quizzes.Any()
                    ? Math.Round(quizzes.Average(q => (double)q.CorrectAnswers / q.TotalQuestions) * 100, 2)
                    : 0;

                friendDtos.Add(new FriendDto
                {
                    UserId = friendId,
                    Username = friend.Username,
                    Email = friend.Email,
                    FriendsSince = friendship.CreatedAt,
                    Stats = new FriendStatsDto
                    {
                        TotalQuizzes = quizCount,
                        CurrentStreak = streak?.CurrentStreak ?? 0,
                        OverallSuccessRate = successRate
                    }
                });
            }

            return friendDtos.OrderBy(f => f.Username).ToList();
        }

        public async Task<bool> RemoveFriendAsync(int userId, int friendId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.User1Id == userId && f.User2Id == friendId) ||
                    (f.User1Id == friendId && f.User2Id == userId));

            if (friendship == null)
                return false;

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FriendDto>> SearchUsersAsync(string searchTerm, int currentUserId)
        {
            var users = await _context.Users
                .Where(u => u.Id != currentUserId &&
                           (u.Username.Contains(searchTerm) || u.Email.Contains(searchTerm)))
                .Take(20)
                .ToListAsync();

            var result = new List<FriendDto>();

            foreach (var user in users)
            {
                var isFriend = await AreFriendsAsync(currentUserId, user.Id);
                if (!isFriend)
                {
                    result.Add(new FriendDto
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FriendsSince = DateTime.MinValue,
                        Stats = new FriendStatsDto()
                    });
                }
            }

            return result;
        }

        private async Task<bool> AreFriendsAsync(int userId1, int userId2)
        {
            return await _context.Friendships
                .AnyAsync(f =>
                    (f.User1Id == userId1 && f.User2Id == userId2) ||
                    (f.User1Id == userId2 && f.User2Id == userId1));
        }
    }
}