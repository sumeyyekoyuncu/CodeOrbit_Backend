using CodeOrbit.Application.DTOs.Quiz;
using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartQuiz(StartQuizDto dto)
        {
            try
            {
                var result = await _quizService.StartQuizAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("answer")]
        public async Task<IActionResult> SubmitAnswer(SubmitQuizAnswerDto dto)
        {
            var result = await _quizService.SubmitAnswerAsync(dto);
            if (!result) return BadRequest("Cevap kaydedilemedi.");
            return Ok("Cevap kaydedildi.");
        }

        [HttpPost("{quizId}/complete")]
        public async Task<IActionResult> CompleteQuiz(int quizId)
        {
            try
            {
                var result = await _quizService.CompleteQuizAsync(quizId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var result = await _quizService.GetUserQuizHistoryAsync(userId);
            return Ok(result);
        }
    }
}
