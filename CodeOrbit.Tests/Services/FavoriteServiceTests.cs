using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using CodeOrbit.Infrastructure.Services;
using CodeOrbit.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CodeOrbit.Tests.Services
{
    public class FavoriteServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly FavoriteService _sut;

        public FavoriteServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new FavoriteService(_context);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region AddToFavoritesAsync

        [Fact]
        public async Task AddToFavoritesAsync_NewFavorite_ReturnsTrueAndPersists()
        {
            // Arrange
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);
            var dto = FavoriteServiceTestHelpers.BuildAddFavoriteDto(userId: 1, questionId: question.Id);

            // Act
            var result = await _sut.AddToFavoritesAsync(dto);

            // Assert
            Assert.True(result);
            var saved = await _context.FavoriteQuestions
                .FirstOrDefaultAsync(f => f.UserId == 1 && f.QuestionId == question.Id);
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task AddToFavoritesAsync_AlreadyFavorited_ReturnsFalseAndDoesNotDuplicate()
        {
            // Arrange
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 1, questionId: question.Id);
            var dto = FavoriteServiceTestHelpers.BuildAddFavoriteDto(userId: 1, questionId: question.Id);

            // Act
            var result = await _sut.AddToFavoritesAsync(dto);

            // Assert
            Assert.False(result);
            var count = await _context.FavoriteQuestions
                .CountAsync(f => f.UserId == 1 && f.QuestionId == question.Id);
            Assert.Equal(1, count); // duplicate olmamalı
        }

        [Fact]
        public async Task AddToFavoritesAsync_SameQuestionDifferentUsers_BothSucceed()
        {
            // Arrange
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);
            var dto1 = FavoriteServiceTestHelpers.BuildAddFavoriteDto(userId: 1, questionId: question.Id);
            var dto2 = FavoriteServiceTestHelpers.BuildAddFavoriteDto(userId: 2, questionId: question.Id);

            // Act
            var result1 = await _sut.AddToFavoritesAsync(dto1);
            var result2 = await _sut.AddToFavoritesAsync(dto2);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
            Assert.Equal(2, await _context.FavoriteQuestions.CountAsync());
        }

        #endregion

        #region RemoveFromFavoritesAsync

        [Fact]
        public async Task RemoveFromFavoritesAsync_ExistingFavorite_ReturnsTrueAndRemoves()
        {
            // Arrange
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 1, questionId: question.Id);

            // Act
            var result = await _sut.RemoveFromFavoritesAsync(userId: 1, questionId: question.Id);

            // Assert
            Assert.True(result);
            var exists = await _context.FavoriteQuestions
                .AnyAsync(f => f.UserId == 1 && f.QuestionId == question.Id);
            Assert.False(exists);
        }

        [Fact]
        public async Task RemoveFromFavoritesAsync_NotFavorited_ReturnsFalse()
        {
            // Arrange — hiç favori eklenmemiş
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);

            // Act
            var result = await _sut.RemoveFromFavoritesAsync(userId: 1, questionId: question.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveFromFavoritesAsync_OnlyRemovesCorrectUsersFavorite()
        {
            // Arrange — iki farklı user aynı soruyu favoriledi
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 1, questionId: question.Id);
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 2, questionId: question.Id);

            // Act — sadece user1'inki silinmeli
            await _sut.RemoveFromFavoritesAsync(userId: 1, questionId: question.Id);

            // Assert
            Assert.False(await _context.FavoriteQuestions.AnyAsync(f => f.UserId == 1 && f.QuestionId == question.Id));
            Assert.True(await _context.FavoriteQuestions.AnyAsync(f => f.UserId == 2 && f.QuestionId == question.Id));
        }

        #endregion

        #region GetUserFavoritesAsync

        [Fact]
        public async Task GetUserFavoritesAsync_NoFavorites_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetUserFavoritesAsync(userId: 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserFavoritesAsync_ReturnsOnlyCurrentUsersFavorites()
        {
            // Arrange
            var q1 = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context, "Soru 1");
            var q2 = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context, "Soru 2");
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 1, questionId: q1.Id);
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 2, questionId: q2.Id); // başka user

            // Act
            var result = await _sut.GetUserFavoritesAsync(userId: 1);

            // Assert
            Assert.Single(result);
            Assert.Equal(q1.Id, result[0].QuestionId);
        }

        [Fact]
        public async Task GetUserFavoritesAsync_ReturnsMappedDtoFieldsCorrectly()
        {
            // Arrange
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context, "Döngü sorusu", DifficultyLevel.Medium);
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 1, questionId: question.Id);

            // Act
            var result = await _sut.GetUserFavoritesAsync(userId: 1);

            // Assert
            var dto = result[0];
            Assert.Equal(question.Id, dto.QuestionId);
            Assert.Equal("Döngü sorusu", dto.QuestionText);
            Assert.Equal("Test Kategori", dto.CategoryName);
            Assert.Equal("Medium", dto.DifficultyLevel);
            Assert.Equal("MultipleChoice", dto.QuestionType);
        }

        [Fact]
        public async Task GetUserFavoritesAsync_ReturnsOrderedByAddedAtDescending()
        {
            // Arrange — farklı zamanlarda eklenen favoriler
            var q1 = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context, "Eski soru");
            var q2 = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context, "Yeni soru");

            _context.FavoriteQuestions.Add(new Domain.Entities.FavoriteQuestion
            {
                UserId = 1,
                QuestionId = q1.Id,
                AddedAt = DateTime.UtcNow.AddDays(-2)
            });
            _context.FavoriteQuestions.Add(new Domain.Entities.FavoriteQuestion
            {
                UserId = 1,
                QuestionId = q2.Id,
                AddedAt = DateTime.UtcNow.AddDays(-1)
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserFavoritesAsync(userId: 1);

            // Assert — en yeni önce gelmeli
            Assert.Equal(q2.Id, result[0].QuestionId);
            Assert.Equal(q1.Id, result[1].QuestionId);
        }

        #endregion

        #region IsFavoriteAsync

        [Fact]
        public async Task IsFavoriteAsync_QuestionIsFavorited_ReturnsTrue()
        {
            // Arrange
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 1, questionId: question.Id);

            // Act
            var result = await _sut.IsFavoriteAsync(userId: 1, questionId: question.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsFavoriteAsync_QuestionNotFavorited_ReturnsFalse()
        {
            // Arrange
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);

            // Act
            var result = await _sut.IsFavoriteAsync(userId: 1, questionId: question.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsFavoriteAsync_OtherUserFavorited_ReturnsFalseForCurrentUser()
        {
            // Arrange — user2 favoriledi, user1 için sorgulanıyor
            var question = await FavoriteServiceTestHelpers.SeedQuestionAsync(_context);
            await FavoriteServiceTestHelpers.SeedFavoriteAsync(_context, userId: 2, questionId: question.Id);

            // Act
            var result = await _sut.IsFavoriteAsync(userId: 1, questionId: question.Id);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}