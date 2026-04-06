using CodeOrbit.Application.DTOs.Leaderboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface ILeaderboardCacheService
    {
        Task<List<LeaderboardEntryDto>?> GetGlobalLeaderboardAsync();
        Task SetGlobalLeaderboardAsync(List<LeaderboardEntryDto> data);

        Task<List<LeaderboardEntryDto>?> GetWeeklyLeaderboardAsync();
        Task SetWeeklyLeaderboardAsync(List<LeaderboardEntryDto> data);

        Task<List<LeaderboardEntryDto>?> GetStreakLeaderboardAsync();
        Task SetStreakLeaderboardAsync(List<LeaderboardEntryDto> data);

        Task InvalidateAllAsync();
    }
}
