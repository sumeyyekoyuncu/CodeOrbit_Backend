using CodeOrbit.Application.DTOs.Leaderboard;
using CodeOrbit.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodeOrbit.Infrastructure.Services
{
    public class RedisLeaderboardCacheService : ILeaderboardCacheService
    {
        private readonly IDistributedCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        private const string GlobalKey = "leaderboard:global";
        private const string WeeklyKey = "leaderboard:weekly";
        private const string StreakKey = "leaderboard:streak";

        public RedisLeaderboardCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<List<LeaderboardEntryDto>?> GetGlobalLeaderboardAsync()
            => await GetAsync(GlobalKey);

        public async Task SetGlobalLeaderboardAsync(List<LeaderboardEntryDto> data)
            => await SetAsync(GlobalKey, data);

        public async Task<List<LeaderboardEntryDto>?> GetWeeklyLeaderboardAsync()
            => await GetAsync(WeeklyKey);

        public async Task SetWeeklyLeaderboardAsync(List<LeaderboardEntryDto> data)
            => await SetAsync(WeeklyKey, data);

        public async Task<List<LeaderboardEntryDto>?> GetStreakLeaderboardAsync()
            => await GetAsync(StreakKey);

        public async Task SetStreakLeaderboardAsync(List<LeaderboardEntryDto> data)
            => await SetAsync(StreakKey, data);

        public async Task InvalidateAllAsync()
        {
            await _cache.RemoveAsync(GlobalKey);
            await _cache.RemoveAsync(WeeklyKey);
            await _cache.RemoveAsync(StreakKey);
        }

        // --- Yardımcı metodlar ---

        private async Task<List<LeaderboardEntryDto>?> GetAsync(string key)
        {
            var json = await _cache.GetStringAsync(key);
            if (json == null) return null;
            return JsonSerializer.Deserialize<List<LeaderboardEntryDto>>(json);
        }

        private async Task SetAsync(string key, List<LeaderboardEntryDto> data)
        {
            var json = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            });
        }
    }
}
