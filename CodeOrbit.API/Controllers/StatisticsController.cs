using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserStatistics(int userId)
        {
            var result = await _statisticsService.GetUserStatisticsAsync(userId);
            return Ok(result);
        }
    }
}