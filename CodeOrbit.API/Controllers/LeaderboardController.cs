using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet("global/{currentUserId}")]
        public async Task<IActionResult> GetGlobalLeaderboard(int currentUserId, [FromQuery] int top = 100)
        {
            var result = await _leaderboardService.GetGlobalLeaderboardAsync(currentUserId, top);
            return Ok(result);
        }

        [HttpGet("weekly/{currentUserId}")]
        public async Task<IActionResult> GetWeeklyLeaderboard(int currentUserId, [FromQuery] int top = 100)
        {
            var result = await _leaderboardService.GetWeeklyLeaderboardAsync(currentUserId, top);
            return Ok(result);
        }

        [HttpGet("streak/{currentUserId}")]
        public async Task<IActionResult> GetStreakLeaderboard(int currentUserId, [FromQuery] int top = 100)
        {
            var result = await _leaderboardService.GetStreakLeaderboardAsync(currentUserId, top);
            return Ok(result);
        }

        [HttpGet("category/{categoryId}/{currentUserId}")]
        public async Task<IActionResult> GetCategoryLeaderboard(int categoryId, int currentUserId, [FromQuery] int top = 50)
        {
            var result = await _leaderboardService.GetCategoryLeaderboardAsync(categoryId, currentUserId, top);
            return Ok(result);
        }

        [HttpGet("all-categories/{currentUserId}")]
        public async Task<IActionResult> GetAllCategoryLeaderboards(int currentUserId)
        {
            var result = await _leaderboardService.GetAllCategoryLeaderboardsAsync(currentUserId);
            return Ok(result);
        }
    }
}