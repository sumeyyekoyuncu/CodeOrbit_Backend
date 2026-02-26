using CodeOrbit.Application.DTOs.Friend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface IFriendService
    {
        Task<bool> SendFriendRequestAsync(SendFriendRequestDto dto);
        Task<bool> RespondToRequestAsync(int userId, RespondToRequestDto dto);
        Task<List<FriendRequestDto>> GetPendingRequestsAsync(int userId);
        Task<List<FriendDto>> GetFriendsAsync(int userId);
        Task<bool> RemoveFriendAsync(int userId, int friendId);
        Task<List<FriendDto>> SearchUsersAsync(string searchTerm, int currentUserId);
    }
}
