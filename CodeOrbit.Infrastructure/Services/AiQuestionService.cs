using CodeOrbit.Application.DTOs.Question;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace CodeOrbit.Infrastructure.Services
{
    public class AiQuestionService : IAiQuestionService
    {
        private readonly string _apiKey;
        private readonly IQuestionService _questionService;
        private readonly HttpClient _httpClient;

        public AiQuestionService(IConfiguration configuration, IQuestionService questionService)
        {
            _apiKey = configuration["Anthropic:ApiKey"] ?? "";
            _questionService = questionService;
            _httpClient = new HttpClient();
        }

        public async Task<List<QuestionDto>> GenerateQuestionsAsync(GenerateQuestionRequestDto request)
        {
            var prompt = "Generate " + request.Count + " " + request.QuestionType +
                         " questions about " + request.CategoryName +
                         " programming at " + request.DifficultyLevel + " difficulty level.\n\n" +
                         "Return ONLY a valid JSON array with no explanation and no markdown. Format:\n" +
                         "[\n" +
                         "  {\n" +
                         "    \"questionText\": \"Question here?\",\n" +
                         "    \"options\": [\"Option A\", \"Option B\", \"Option C\", \"Option D\"],\n" +
                         "    \"correctOptionIndex\": 0\n" +
                         "  }\n" +
                         "]\n\n" +
                         "Rules:\n" +
                         "- Questions must be in Turkish\n" +
                         "- Options must be in Turkish or code\n" +
                         "- correctOptionIndex is 0-based index of correct answer\n" +
                         "- Make questions practical and educational\n" +
                         "- Do NOT wrap response in markdown code blocks\n" +
                         "- Difficulty: " + request.DifficultyLevel;

            var requestBody = new
            {
                model = "claude-sonnet-4-5",
                max_tokens = 2000,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
            var responseStr = await response.Content.ReadAsStringAsync();

            await File.WriteAllTextAsync("C:\\Temp\\claude.txt", responseStr);

            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseStr);

            if (!responseJson.TryGetProperty("content", out var contentArray))
                return new List<QuestionDto>();

            var responseText = contentArray[0].GetProperty("text").GetString() ?? "[]";
            responseText = responseText.Replace("```json", "").Replace("```", "").Trim();

            var startIndex = responseText.IndexOf('[');
            var endIndex = responseText.LastIndexOf(']');
            if (startIndex == -1 || endIndex == -1) return new List<QuestionDto>();

            var jsonStr = responseText.Substring(startIndex, endIndex - startIndex + 1);
            var aiQuestions = JsonSerializer.Deserialize<List<AiGeneratedQuestion>>(jsonStr, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (aiQuestions == null) return new List<QuestionDto>();

            var savedQuestions = new List<QuestionDto>();
            foreach (var aiQ in aiQuestions)
            {
                try
                {
                    var createDto = new CreateQuestionDto
                    {
                        CategoryId = request.CategoryId,
                        QuestionText = aiQ.QuestionText,
                        QuestionType = Enum.Parse<QuestionType>(request.QuestionType, ignoreCase: true),
                        DifficultyLevel = Enum.Parse<DifficultyLevel>(request.DifficultyLevel, ignoreCase: true),
                        Options = aiQ.Options.Select((opt, index) => new CreateOptionDto
                        {
                            OptionText = opt,
                            IsCorrect = index == aiQ.CorrectOptionIndex
                        }).ToList()
                    };

                    var saved = await _questionService.CreateAsync(createDto);
                    savedQuestions.Add(saved);
                }
                catch (Exception ex)
                {
                    await File.AppendAllTextAsync("C:\\Temp\\claude.txt",
                        "\n\nDB ERROR: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
                }
            }

            return savedQuestions;
        }
    }

    public class AiGeneratedQuestion
    {
        public string QuestionText { get; set; } = "";
        public List<string> Options { get; set; } = new();
        public int CorrectOptionIndex { get; set; }
    }
}