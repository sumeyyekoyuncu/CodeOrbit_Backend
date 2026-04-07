using System.Net;
using System.Net.Http.Json;
using CodeOrbit.Application.DTOs.Challenge;
using CodeOrbit.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CodeOrbit.Tests.Integration
{
    public class ChallengeIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        #region Setup & Teardown

        private readonly CustomWebApplicationFactory _factory;
        private HttpClient _client = null!;
        private IServiceScope _scope = null!;
        private AppDbContext _db = null!;
        private int _userId;

        public ChallengeIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            _scope = _factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _client = _factory.CreateClient();

            var email = $"test_{Guid.NewGuid()}@test.com";
            var user = await IntegrationTestHelpers.SeedUserAsync(_scope, email: email);
            _userId = user.Id;
            var token = await IntegrationTestHelpers.GetTokenAsync(_client, email: email);
            IntegrationTestHelpers.SetAuthHeader(_client, token);
        }

        public Task DisposeAsync()
        {
            _scope.Dispose();
            _client.Dispose();
            return Task.CompletedTask;
        }

  private async Task<DailyChallengeDto> SetupChallengeAsync()
{
    var category = await Helpers.ChallengeServiceTestHelpers.SeedCategoryAsync(_db);
    var questions = await Helpers.ChallengeServiceTestHelpers.SeedQuestionsAsync(_db, category.Id);

    await Helpers.ChallengeServiceTestHelpers.SeedDailyChallengeAsync(_db, category.Id, questions);

    var response = await _client.GetAsync($"/api/Challenge/today/{_userId}");
    response.EnsureSuccessStatusCode();

    var challenge = await response.Content.ReadFromJsonAsync<DailyChallengeDto>();

    return challenge!;
}

        // Challenge için tüm doğru cevapları oluşturur
        private SubmitChallengeDto BuildSubmitDto(DailyChallengeDto challenge, bool allCorrect = true)
        {
            return new SubmitChallengeDto
            {
                UserId = _userId,
                DailyChallengeId = challenge.ChallengeId,
                Answers = challenge.Questions.Select(q => new ChallengeAnswerDto
                {
                    QuestionId = q.QuestionId,
                    SelectedOptionId = _db.Options
                        .First(o => o.QuestionId == q.QuestionId && o.IsCorrect == allCorrect).Id
                }).ToList()
            };
        }

        #endregion

        #region GET /api/Challenge/today/{userId}

        [Fact]
        public async Task GetTodaysChallenge_WithQuestionsSeeded_Returns200WithChallenge()
        {
            // Arrange
            var challenge = await SetupChallengeAsync();

            // Assert
            Assert.NotNull(challenge);
            Assert.Equal(10, challenge.TotalQuestions);
            Assert.False(challenge.HasCompleted);
            Assert.Equal(DateTime.UtcNow.Date, challenge.Date.Date);
        }

        #endregion

        #region POST /api/Challenge/submit

        [Fact]
        public async Task SubmitChallenge_AllCorrect_Returns200WithFullScore()
        {
            // Arrange
            var challenge = await SetupChallengeAsync();
            var submitDto = BuildSubmitDto(challenge, allCorrect: true);

            // Act
            var response = await _client.PostAsJsonAsync("/api/Challenge/submit", submitDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<ChallengeResultDto>();
            Assert.NotNull(result);
            Assert.Equal(10, result!.TotalQuestions);
            Assert.Equal(10, result.CorrectAnswers);
            Assert.Equal(100.0, result.SuccessRate);
        }

        [Fact]
        public async Task SubmitChallenge_AlreadySubmitted_Returns400()
        {
            // Arrange
            var challenge = await SetupChallengeAsync();
            var submitDto = BuildSubmitDto(challenge);

            // İlk submit
            await _client.PostAsJsonAsync("/api/Challenge/submit", submitDto);

            // Act — ikinci submit
            var response = await _client.PostAsJsonAsync("/api/Challenge/submit", submitDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region GET /api/Challenge/leaderboard

        [Fact]
        public async Task GetLeaderboard_WithAttempts_Returns200WithEntries()
        {
            // Arrange — challenge oluştur ve submit et
            var challenge = await SetupChallengeAsync();
            var submitDto = BuildSubmitDto(challenge);
            await _client.PostAsJsonAsync("/api/Challenge/submit", submitDto);

            // Act
            var response = await _client.GetAsync("/api/Challenge/leaderboard");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<List<ChallengeLeaderboardDto>>();
            Assert.NotNull(result);
            Assert.NotEmpty(result!);
        }

        #endregion
    }
}
