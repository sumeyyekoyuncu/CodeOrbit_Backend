using CodeOrbit.Application.DTOs.UserProgress;
using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserProgressController : ControllerBase
    {
        private readonly IUserProgressService _userProgressService;

        public UserProgressController(IUserProgressService userProgressService)
        {
            _userProgressService = userProgressService;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit(SubmitAnswerDto dto)
        {
            var result = await _userProgressService.SubmitAnswerAsync(dto);
            if (!result) return BadRequest("Geçersiz seçenek.");
            return Ok("Cevap kaydedildi.");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProgress(int userId)
        {
            var result = await _userProgressService.GetUserProgressAsync(userId);
            return Ok(result);
        }
    }
}
