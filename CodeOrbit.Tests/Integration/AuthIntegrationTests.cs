using CodeOrbit.Application.DTOs.Auth;
using CodeOrbit.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CodeOrbit.Tests.Integration
{
    public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        #region Setup & Teardown

        private readonly CustomWebApplicationFactory _factory;
        private HttpClient _client = null!;
        private IServiceScope _scope = null!;

        public AuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        public Task InitializeAsync()
        {
            _scope = _factory.Services.CreateScope();
            _client = _factory.CreateClient();
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _scope.Dispose();
            _client.Dispose();
            return Task.CompletedTask;
        }

        #endregion
        #region POST /api/Auth/register

        [Fact]
        public async Task Register_ValidRequest_Returns200WithToken()
        {
            // Arrange
            var dto = new
            {
                Username = "testuser",
                Email = $"test_{Guid.NewGuid()}@test.com", // unique email
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400()
        {
            // Arrange — aynı email ile iki kez register
            var email = $"test_{Guid.NewGuid()}@test.com";
            var dto = new
            {
                Username = "testuser",
                Email = email,
                Password = "Password123!"
            };

            await _client.PostAsJsonAsync("/api/Auth/register", dto);

            // Act — aynı email ile tekrar
            var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion
        #region POST /api/Auth/login

        [Fact]
        public async Task Login_ValidCredentials_Returns200WithToken()
        {
            // Arrange — önce register, sonra login
            var email = $"test_{Guid.NewGuid()}@test.com";
            var registerDto = new
            {
                Username = "testuser",
                Email = email,
                Password = "Password123!"
            };
            await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            var loginDto = new
            {
                Email = email,
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task Login_WrongPassword_Returns400()
        {
            // Arrange
            var email = $"test_{Guid.NewGuid()}@test.com";
            var registerDto = new
            {
                Username = "testuser",
                Email = email,
                Password = "Password123!"
            };
            await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            var loginDto = new
            {
                Email = email,
                Password = "WrongPassword!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_NonExistentEmail_Returns400()
        {
            // Arrange — hiç register olmamış email
            var loginDto = new
            {
                Email = "nonexistent@test.com",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion
    }
}