using CodeOrbit.Application.DTOs.Auth;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;
using CodeOrbit.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CodeOrbit.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _jwtServiceMock = new Mock<IJwtService>();

            // Her token üretim çağrısında sabit bir değer döndür
            _jwtServiceMock
                .Setup(j => j.GenerateToken(It.IsAny<User>()))
                .Returns("mocked-jwt-token");

            _sut = new AuthService(_context, _jwtServiceMock.Object);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region RegisterAsync

        [Fact]
        public async Task RegisterAsync_ValidDto_ReturnsAuthResponse()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("mocked-jwt-token", result.Token);
        }

        [Fact]
        public async Task RegisterAsync_ValidDto_SavesUserToDatabase()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            await _sut.RegisterAsync(dto);

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            Assert.NotNull(savedUser);
            Assert.Equal("testuser", savedUser.Username);
        }

        [Fact]
        public async Task RegisterAsync_ValidDto_StoresHashedPasswordNotPlainText()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            await _sut.RegisterAsync(dto);

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            Assert.NotEqual("Password123!", savedUser!.PasswordHash); // düz metin olmamalı
            Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", savedUser.PasswordHash)); // hash doğrulanmalı
        }

        [Fact]
        public async Task RegisterAsync_ValidDto_CallsGenerateTokenOnce()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            await _sut.RegisterAsync(dto);

            // Assert — token üretimi tam 1 kez çağrılmalı
            _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsException()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "existing",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123!")
            });
            await _context.SaveChangesAsync();

            var dto = new RegisterDto
            {
                Username = "newuser",
                Email = "test@example.com", // aynı email
                Password = "Password123!"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.RegisterAsync(dto));
            Assert.Equal("Bu email zaten kayıtlı.", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_DoesNotCallGenerateToken()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "existing",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123!")
            });
            await _context.SaveChangesAsync();

            var dto = new RegisterDto
            {
                Username = "newuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => _sut.RegisterAsync(dto));

            // Assert — hata fırlatıldıysa token üretilmemiş olmalı
            _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region LoginAsync

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
            _context.Users.Add(new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = passwordHash
            });
            await _context.SaveChangesAsync();

            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("mocked-jwt-token", result.Token);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_CallsGenerateTokenOnce()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            });
            await _context.SaveChangesAsync();

            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            await _sut.LoginAsync(dto);

            // Assert
            _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WrongEmail_ThrowsException()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(dto));
            Assert.Equal("Email veya şifre hatalı.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsException()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass123!")
            });
            await _context.SaveChangesAsync();

            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPass999!" // yanlış şifre
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(dto));
            Assert.Equal("Email veya şifre hatalı.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_DoesNotCallGenerateToken()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass123!")
            });
            await _context.SaveChangesAsync();

            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPass999!"
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(dto));

            // Assert
            _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        #endregion
    }
}