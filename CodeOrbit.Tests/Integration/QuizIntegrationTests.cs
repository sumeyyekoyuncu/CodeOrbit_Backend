using Azure;
using CodeOrbit.Application.DTOs.Quiz;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Tests.Integration
{
    public class QuizIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        #region Setup & Teardown

        private readonly CustomWebApplicationFactory _factory;
        private HttpClient _client = null!;
        private IServiceScope _scope = null!;
        private AppDbContext _db = null!;
        private string _token = null!;

        public QuizIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private int _userId;

        public async Task InitializeAsync()
        {
            _scope = _factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _client = _factory.CreateClient();

            var email = $"test_{Guid.NewGuid()}@test.com";
            var user = await IntegrationTestHelpers.SeedUserAsync(_scope, email: email);
            _userId = user.Id; // gerçek ID'yi sakla
            _token = await IntegrationTestHelpers.GetTokenAsync(_client, email: email);
            IntegrationTestHelpers.SetAuthHeader(_client, _token);
        }
        public Task DisposeAsync()
        {
            _scope.Dispose();
            _client.Dispose();
            return Task.CompletedTask;
        }

        #endregion
        #region POST /api/quiz/start
        [Fact]
        public async Task StartQuiz_ValidRequest_Returns200WithQuizDto()
        {
            //Arrange
            var authHeader = _client.DefaultRequestHeaders.Authorization;
            var (category, _) = await IntegrationTestHelpers
                .SeedCategoryWithQuestionsAsync(_scope, QuestionCount: 5);
            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };
            //Act
            var response = await _client.PostAsJsonAsync("/api/Quiz/start", dto);
            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<QuizDto>();
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalQuestions);
            Assert.Equal(3, result.Questions.Count);
            Assert.Equal(category.Name, result.CategoryName);
        }
        [Fact]
        public async Task StartQuiz_UnsufficientQuestions_Returns400()
        {
            //Arrange
            var (category, _) = await IntegrationTestHelpers
                .SeedCategoryWithQuestionsAsync(_scope, QuestionCount: 2);
            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 5
            };
            //Act
            var response = await _client.PostAsJsonAsync("/api/Quiz/start", dto);
            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task StartQuiz_Unauthorized_Returns401()
        {
            var unauthorizedClient = _factory.CreateClient();
            var dto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = 1,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };
            var response = await unauthorizedClient.PostAsJsonAsync("/api/Quiz/start", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        #endregion
        #region POST /api/quiz/submit-answer
        [Fact]
        public async Task SubmitAnswer_CorrectAnswer_Returns200True()
        {
            // Arrange
            var (category, _) = await IntegrationTestHelpers
                .SeedCategoryWithQuestionsAsync(_scope, QuestionCount: 3);

            var startDto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };

            var startResponse = await _client.PostAsJsonAsync("/api/Quiz/start", startDto);
            var quiz = await startResponse.Content.ReadFromJsonAsync<QuizDto>();

            var firstQuestion = quiz!.Questions[0];
            var correctOption = _db.Options
                .First(o => o.QuestionId == firstQuestion.QuestionId && o.IsCorrect);

            var submitDto = new SubmitQuizAnswerDto
            {
                QuizId = quiz.QuizId,
                QuizQuestionId = firstQuestion.QuizQuestionId,
                SelectedOptionId = correctOption.Id
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Quiz/answer", submitDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("Cevap kaydedildi.", result.Trim('"'));
        }
        #endregion
        #region POST /api/Quiz/{quizId}/complete
        [Fact]
        public async Task CompleteQuiz_ValidRequest_Returns200WithResult()
        {
            // Arrange
            var (category, _) = await IntegrationTestHelpers
                .SeedCategoryWithQuestionsAsync(_scope, QuestionCount: 3);
            var startDto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };
            var startResponse = await _client.PostAsJsonAsync("/api/Quiz/start", startDto);
            var quiz = await startResponse.Content.ReadFromJsonAsync<QuizDto>();
            foreach (var question in quiz!.Questions)
            {
                var correctOption = _db.Options
                    .First(o => o.QuestionId == question.QuestionId && o.IsCorrect);

                await _client.PostAsJsonAsync("/api/Quiz/answer", new SubmitQuizAnswerDto
                {
                    QuizId = quiz.QuizId,
                    QuizQuestionId = question.QuizQuestionId,
                    SelectedOptionId = correctOption.Id
                });
            }

            // Act
            var response = await _client.PostAsync($"/api/Quiz/{quiz!.QuizId}/complete", null);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<QuizResultDto>();
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalQuestions);
            Assert.Equal(3, result.CorrectAnswers);
            Assert.Equal(100.0, result.SuccessRate);
        }
        [Fact]
        public async Task CompleteQuiz_AlreadyCompleted_Returns400()
        {
           //Arrange
           var (category,_) = await IntegrationTestHelpers
               .SeedCategoryWithQuestionsAsync(_scope, QuestionCount: 3);
            var startDto = new StartQuizDto
            {
                UserId = 1,
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };
            var startResponse = await _client.PostAsJsonAsync("/api/Quiz/start", startDto);
            var quiz = await startResponse.Content.ReadFromJsonAsync<QuizDto>();
            await _client.PostAsync($"/api/Quiz/{quiz!.QuizId}/complete", null);

            //Act
            var response = await _client.PostAsync($"/api/Quiz/{quiz.QuizId}/complete", null);
            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task CompleteQuiz_QuizNotFound_Returns400()
        {
            var response = await _client.PostAsync("/api/Quiz/9999/complete", null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion
        #region GET /api/Quiz/history/{userId}

        [Fact]
        public async Task GetQuizHistory_CompletedQuiz_Returns200WithHistory()
        {
            // Arrange — quiz başlat ve tamamla
            var (category, _) = await IntegrationTestHelpers
                .SeedCategoryWithQuestionsAsync(_scope, QuestionCount: 3);

            var startDto = new StartQuizDto
            {
                UserId = _userId, // sabit 1 değil, seed'den gelen gerçek ID
                CategoryId = category.Id,
                DifficultyLevel = DifficultyLevel.Easy,
                QuestionCount = 3
            };

            var startResponse = await _client.PostAsJsonAsync("/api/Quiz/start", startDto);
            var quiz = await startResponse.Content.ReadFromJsonAsync<QuizDto>();
            await _client.PostAsJsonAsync($"/api/Quiz/{quiz!.QuizId}/complete", (object?)null);

            // Act
            var response = await _client.GetAsync($"/api/Quiz/history/{_userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var history = await response.Content.ReadFromJsonAsync<List<QuizHistoryDto>>();
            Assert.Single(history!);
            Assert.Equal(category.Name, history![0].CategoryName);
        }

        [Fact]
        public async Task GetQuizHistory_NoCompletedQuizzes_ReturnsEmptyList()
        {
            // Arrange — hiç quiz tamamlanmamış

            // Act
            var response = await _client.GetAsync($"/api/Quiz/history/{_userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var history = await response.Content.ReadFromJsonAsync<List<QuizHistoryDto>>();
            Assert.Empty(history!);
        }

        #endregion

    }
}

