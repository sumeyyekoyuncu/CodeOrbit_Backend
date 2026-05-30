using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CodeOrbit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodobiController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public CodobiController(IStatisticsService statisticsService, IConfiguration configuration)
        {
            _statisticsService = statisticsService;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] CodobiChatRequest request)
        {
            try
            {
                var apiKey = _configuration["Anthropic:ApiKey"] ?? "";

                // Kullanıcı istatistiklerini al
                var statsContext = "";
                try
                {
                    var stats = await _statisticsService.GetUserStatisticsAsync(request.UserId);
                    statsContext = $"Kullanıcı istatistikleri: Toplam quiz: {stats.TotalQuizzes}, " +
                                   $"Başarı oranı: {stats.OverallSuccessRate:F0}%, " +
                                   $"Mevcut seri: {stats.CurrentStreak} gün.";
                }
                catch { }

                var systemPrompt = "Sen Codobi'sin, CodeOrbit uygulamasının yapay zeka programlama asistanı. " +
                                   "Kullanıcılara programlama öğrenmelerinde yardımcı oluyorsun. " +
                                   "Türkçe konuş. Samimi, eğlenceli ve teşvik edici ol. " +
                                   "Kısa ve net cevaplar ver. Emoji kullan. " +
                                   (statsContext.Length > 0 ? $"Kullanıcı bilgileri: {statsContext}" : "");

                var requestBody = new
                {
                    model = "claude-sonnet-4-5",
                    max_tokens = 1000,
                    system = systemPrompt,
                    messages = request.Messages
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
                var responseStr = await response.Content.ReadAsStringAsync();

                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseStr);
                var aiResponse = responseJson
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "Üzgünüm, bir hata oluştu.";

                return Ok(new { response = aiResponse });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class CodobiChatRequest
    {
        public int UserId { get; set; }
        public List<CodobiMessage> Messages { get; set; } = new();
    }

    public class CodobiMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }
}