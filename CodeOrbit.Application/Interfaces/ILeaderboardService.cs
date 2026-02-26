using CodeOrbit.Application.DTOs.Leaderboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int currentUserId, int top = 100);
        Task<List<LeaderboardEntryDto>> GetWeeklyLeaderboardAsync(int currentUserId, int top = 100);
        Task<List<LeaderboardEntryDto>> GetStreakLeaderboardAsync(int currentUserId, int top = 100);
        Task<CategoryLeaderboardDto> GetCategoryLeaderboardAsync(int categoryId, int currentUserId, int top = 50);
        Task<List<CategoryLeaderboardDto>> GetAllCategoryLeaderboardsAsync(int currentUserId);
    }
}
