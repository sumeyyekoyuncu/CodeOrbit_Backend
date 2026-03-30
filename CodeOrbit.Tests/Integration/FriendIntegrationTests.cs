using System.Net;
using System.Net.Http.Json;
using CodeOrbit.Application.DTOs.Friend;
using CodeOrbit.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CodeOrbit.Tests.Integration
{
    public class FriendIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        #region Setup & Teardown

        private readonly CustomWebApplicationFactory _factory;
        private HttpClient _client = null!;
        private IServiceScope _scope = null!;
        private int _userId;

        public FriendIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            _scope = _factory.Services.CreateScope();
            _client = _factory.CreateClient();

            // Her test için unique kullanıcı
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

        #endregion
        #region POST /api/Friend/send-request

        [Fact]
        public async Task SendFriendRequest_ValidRequest_Returns200()
        {
            // Arrange
            var receiver = await IntegrationTestHelpers.SeedUserAsync(
                _scope,
                username: "receiver",
                email: $"receiver_{Guid.NewGuid()}@test.com");

            var dto = new SendFriendRequestDto
            {
                SenderId = _userId,
                ReceiverId = receiver.Id
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Friend/request", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("Arkadaşlık isteği gönderildi.", result.Trim('"'));
        }

        [Fact]
        public async Task SendFriendRequest_ToSelf_Returns400()
        {
            // Arrange
            var dto = new SendFriendRequestDto
            {
                SenderId = _userId,
                ReceiverId = _userId
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Friend/request", dto);

            // Assert — servis false dönünce controller BadRequest atıyor
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendFriendRequest_DuplicateRequest_Returns400()
        {
            // Arrange
            var receiver = await IntegrationTestHelpers.SeedUserAsync(
                _scope,
                username: "receiver",
                email: $"receiver_{Guid.NewGuid()}@test.com");

            var dto = new SendFriendRequestDto
            {
                SenderId = _userId,
                ReceiverId = receiver.Id
            };

            await _client.PostAsJsonAsync("/api/Friend/request", dto);

            // Act
            var response = await _client.PostAsJsonAsync("/api/Friend/request", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion
        #region POST /api/Friend/respond/{userId}

        [Fact]
        public async Task RespondToRequest_Accept_Returns200()
        {
            // Arrange — istek gönder, sonra kabul et
            var receiver = await IntegrationTestHelpers.SeedUserAsync(
                _scope,
                username: "receiver",
                email: $"receiver_{Guid.NewGuid()}@test.com");

            var sendDto = new SendFriendRequestDto
            {
                SenderId = _userId,
                ReceiverId = receiver.Id
            };
            await _client.PostAsJsonAsync("/api/Friend/request", sendDto);

            // DB'den isteği al
            var db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var request = db.FriendRequests
                .First(fr => fr.SenderId == _userId && fr.ReceiverId == receiver.Id);

            var respondDto = new RespondToRequestDto
            {
                RequestId = request.Id,
                Accept = true
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/Friend/respond/{receiver.Id}", respondDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("İstek kabul edildi.", result.Trim('"'));
        }

        [Fact]
        public async Task RespondToRequest_Reject_Returns200()
        {
            // Arrange
            var receiver = await IntegrationTestHelpers.SeedUserAsync(
                _scope,
                username: "receiver",
                email: $"receiver_{Guid.NewGuid()}@test.com");

            var sendDto = new SendFriendRequestDto
            {
                SenderId = _userId,
                ReceiverId = receiver.Id
            };
            await _client.PostAsJsonAsync("/api/Friend/request", sendDto);

            var db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var request = db.FriendRequests
                .First(fr => fr.SenderId == _userId && fr.ReceiverId == receiver.Id);

            var respondDto = new RespondToRequestDto
            {
                RequestId = request.Id,
                Accept = false
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/Friend/respond/{receiver.Id}", respondDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("İstek reddedildi.", result.Trim('"'));
        }

        #endregion
        #region GET /api/Friend/search

        [Fact]
        public async Task SearchUsers_MatchingUsername_ReturnsUser()
        {
            // Arrange
            await IntegrationTestHelpers.SeedUserAsync(
                _scope,
                username: "john_doe",
                email: $"john_{Guid.NewGuid()}@test.com");

            // Act
            var response = await _client.GetAsync(
                $"/api/Friend/search?searchTerm=john&currentUserId={_userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<List<FriendDto>>();
            Assert.Single(result!);
            Assert.Equal("john_doe", result![0].Username);
        }

        [Fact]
        public async Task SearchUsers_NoMatch_ReturnsEmptyList()
        {
            // Act
            var response = await _client.GetAsync(
                $"/api/Friend/search?searchTerm=zzznomatch&currentUserId={_userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<List<FriendDto>>();
            Assert.Empty(result!);
        }

        [Fact]
        public async Task SearchUsers_DoesNotReturnCurrentUser()
        {
            // Act — kendi username'ini ara
            var response = await _client.GetAsync(
                $"/api/Friend/search?searchTerm=testuser&currentUserId={_userId}");

            // Assert — kendini görmemeli
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<List<FriendDto>>();
            Assert.DoesNotContain(result!, r => r.UserId == _userId);
        }

        #endregion
    }
}