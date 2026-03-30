using CodeOrbit.Application.DTOs.Friend;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Helpers
{
    public static class FriendServiceTestHelpers
    {
        public static async Task<User> SeedUserAsync(
            AppDbContext context,
            string username,
            string email)
        {
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public static async Task<FriendRequest> SeedFriendRequestAsync(
            AppDbContext context,
            int senderId,
            int receiverId,
            FriendRequestStatus status = FriendRequestStatus.Pending)
        {
            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = status,
                SentAt = DateTime.UtcNow
            };
            context.FriendRequests.Add(request);
            await context.SaveChangesAsync();
            return request;
        }

        public static async Task<Friendship> SeedFriendshipAsync(
            AppDbContext context,
            int userId1,
            int userId2)
        {
            var friendship = new Friendship
            {
                User1Id = Math.Min(userId1, userId2),
                User2Id = Math.Max(userId1, userId2),
                CreatedAt = DateTime.UtcNow
            };
            context.Friendships.Add(friendship);
            await context.SaveChangesAsync();
            return friendship;
        }

        public static SendFriendRequestDto BuildSendRequestDto(int senderId, int receiverId) =>
            new SendFriendRequestDto { SenderId = senderId, ReceiverId = receiverId };

        public static RespondToRequestDto BuildRespondDto(int requestId, bool accept) =>
            new RespondToRequestDto { RequestId = requestId, Accept = accept };
    }
}