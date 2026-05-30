using CodeOrbit.Application.DTOs.Question;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IAiQuestionService _aiQuestionService;

        public QuestionController(IQuestionService questionService, IAiQuestionService aiQuestionService)
        {
            _questionService = questionService;
            _aiQuestionService = aiQuestionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _questionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _questionService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var result = await _questionService.GetByCategoryAsync(categoryId);
            return Ok(result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
            [FromQuery] ProgrammingLanguage? language,
            [FromQuery] DifficultyLevel? difficulty,
            [FromQuery] QuestionType? type)
        {
            var result = await _questionService.FilterAsync(language, difficulty, type);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateQuestionDto dto)
        {
            var result = await _questionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _questionService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateWithAi([FromBody] GenerateQuestionRequestDto request)
        {
            try
            {
                var result = await _aiQuestionService.GenerateQuestionsAsync(request);
                return Ok(new
                {
                    message = $"{result.Count} soru başarıyla üretildi ve kaydedildi.",
                    questions = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}