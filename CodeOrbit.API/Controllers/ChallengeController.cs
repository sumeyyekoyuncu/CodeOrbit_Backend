using CodeOrbit.Application.DTOs.Challenge;
using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeService _challengeService;

        public ChallengeController(IChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        [HttpGet("today/{userId}")]
        public async Task<IActionResult> GetTodaysChallenge(int userId)
        {
            try
            {
                var result = await _challengeService.GetTodaysChallengeAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitChallenge(SubmitChallengeDto dto)
        {
            try
            {
                var result = await _challengeService.SubmitChallengeAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard()
        {
            var result = await _challengeService.GetTodaysLeaderboardAsync();
            return Ok(result);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateChallenge()
        {
            await _challengeService.GenerateDailyChallengeAsync();
            return Ok("Challenge oluşturuldu.");
        }
    }
}