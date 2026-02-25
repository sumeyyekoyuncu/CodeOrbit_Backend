using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Application.DTOs.Auth;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Services;
using CodeOrbit.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace CodeOrbit.Tests
{
    public class AuthServiceTest
    {
        private readonly Mock<IJwtService> _jwtServiceMock;
        public AuthServiceTest()
        {
            _jwtServiceMock = new Mock<IJwtService>();
            _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
                .Returns("mocked-jwt-token");
        }
        [Fact]
        public async Task Register_ShouldCreateUser_WhenEmailIsUnique()
        {
            //arrange
            var context = DbContextHelper.CreateInMemoryContext("Register_Success");
            var service=new AuthService(context, _jwtServiceMock.Object);
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test123!"
            };
            //act
            var result = await service.RegisterAsync(dto);
            //assert
            result.Should().NotBeNull();
            result.Username.Should().Be(dto.Username);
            result.Email.Should().Be(dto.Email);
            result.Token.Should().Be("mocked-jwt-token");
        }
        [Fact]
        public async Task Register_ShouldThrowExpection_WhenEmailAlreadyExists()
        {
            //arrange
            var context = DbContextHelper.CreateInMemoryContext("Register_Duplicate");
            var service = new AuthService(context, _jwtServiceMock.Object);
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "Test@example.com",
                Password = "Test123!"
            };
            await service.RegisterAsync(dto);
            //act
            var act= async () => await service.RegisterAsync(dto);
            //assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Bu email zaten kayıtlı.");
        }
        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            //arrange
            var context = DbContextHelper.CreateInMemoryContext("Login_Success");
            var service = new AuthService(context, _jwtServiceMock.Object);
            await service.RegisterAsync(new RegisterDto
            {
                Username = "testuser",
                Email="Test@example.com",
                Password = "Test123!"
            });
            var loginDto = new LoginDto
            {
                Email = "Test@example.com",
                Password = "Test123!"
            };
            //act
            var result = await service.LoginAsync(loginDto);
            //assert
        result.Should().NotBeNull();
        result.Token.Should().Be("mocked-jwt-token");

        }

    }
}
