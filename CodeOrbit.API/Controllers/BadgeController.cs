using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BadgeController : ControllerBase
    {
        private readonly IBadgeService _badgeService;

        public BadgeController(IBadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserBadges(int userId)
        {
            var result = await _badgeService.GetUserBadgesAsync(userId);
            return Ok(result);
        }

        [HttpPost("check/{userId}")]
        public async Task<IActionResult> CheckBadges(int userId)
        {
            await _badgeService.CheckAndAwardBadgesAsync(userId);
            return Ok("Rozetler kontrol edildi.");
        }
    }
}