using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using CodeOrbit.Infrastructure.Services;
using CodeOrbit.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CodeOrbit.Tests.Services
{
    public class QuestionServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly QuestionService _sut;

        public QuestionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new QuestionService(_context);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_NoQuestions_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllQuestions()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, "Soru 1");
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, "Soru 2");
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, "Soru 3");

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedDtoFieldsCorrectly()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "Python");
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, "Lambda nedir?",
                DifficultyLevel.Medium, QuestionType.MultipleChoice);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            var dto = result[0];
            Assert.Equal("Lambda nedir?", dto.QuestionText);
            Assert.Equal("Python", dto.CategoryName);
            Assert.Equal("Medium", dto.DifficultyLevel);
            Assert.Equal("MultipleChoice", dto.QuestionType);
            Assert.Equal(4, dto.Options.Count);
        }

        #endregion

        #region GetByCategoryAsync

        [Fact]
        public async Task GetByCategoryAsync_NoQuestionsInCategory_ReturnsEmptyList()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);

            // Act
            var result = await _sut.GetByCategoryAsync(category.Id);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByCategoryAsync_ReturnsOnlyQuestionsFromTargetCategory()
        {
            // Arrange
            var csharp = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "C#");
            var python = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "Python");

            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, csharp.Id, "C# sorusu 1");
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, csharp.Id, "C# sorusu 2");
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, python.Id, "Python sorusu");

            // Act
            var result = await _sut.GetByCategoryAsync(csharp.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, q => Assert.Equal("C#", q.CategoryName));
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingQuestion_ReturnsDto()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            var question = await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, "Soru metni");

            // Act
            var result = await _sut.GetByIdAsync(question.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(question.Id, result.Id);
            Assert.Equal("Soru metni", result.QuestionText);
        }

        [Fact]
        public async Task GetByIdAsync_QuestionNotFound_ReturnsNull()
        {
            // Act
            var result = await _sut.GetByIdAsync(id: 999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOptionsWithoutIsCorrect()
        {
            // Arrange — OptionDto'da IsCorrect olmamalı (güvenlik)
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            var question = await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id);

            // Act
            var result = await _sut.GetByIdAsync(question.Id);

            // Assert — options dönüyor ama IsCorrect expose edilmiyor
            Assert.NotEmpty(result!.Options);
            Assert.All(result.Options, o => Assert.NotNull(o.OptionText));
        }

        #endregion

        #region FilterAsync

        [Fact]
        public async Task FilterAsync_NoFilters_ReturnsAllQuestions()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id);

            // Act
            var result = await _sut.FilterAsync(language: null, difficulty: null, type: null);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task FilterAsync_ByLanguage_ReturnsOnlyMatchingQuestions()
        {
            // Arrange
            var csharp = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "C#", ProgrammingLanguage.CSharp);
            var python = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "Python", ProgrammingLanguage.Python);

            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, csharp.Id);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, python.Id);

            // Act
            var result = await _sut.FilterAsync(language: ProgrammingLanguage.CSharp, difficulty: null, type: null);

            // Assert
            Assert.Single(result);
            Assert.Equal("C#", result[0].CategoryName);
        }

        [Fact]
        public async Task FilterAsync_ByDifficulty_ReturnsOnlyMatchingQuestions()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, difficulty: DifficultyLevel.Easy);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, difficulty: DifficultyLevel.Hard);

            // Act
            var result = await _sut.FilterAsync(language: null, difficulty: DifficultyLevel.Easy, type: null);

            // Assert
            Assert.Single(result);
            Assert.Equal("Easy", result[0].DifficultyLevel);
        }

        [Fact]
        public async Task FilterAsync_ByQuestionType_ReturnsOnlyMatchingQuestions()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, type: QuestionType.MultipleChoice);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, type: QuestionType.TrueFalse);

            // Act
            var result = await _sut.FilterAsync(language: null, difficulty: null, type: QuestionType.TrueFalse);

            // Assert
            Assert.Single(result);
            Assert.Equal("TrueFalse", result[0].QuestionType);
        }

        [Fact]
        public async Task FilterAsync_ByLanguageAndDifficulty_ReturnsOnlyMatchingQuestions()
        {
            // Arrange
            var csharp = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "C#", ProgrammingLanguage.CSharp);
            var python = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "Python", ProgrammingLanguage.Python);

            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, csharp.Id, difficulty: DifficultyLevel.Easy);   // ✅ eşleşmeli
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, csharp.Id, difficulty: DifficultyLevel.Hard);   // ❌ difficulty uyumsuz
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, python.Id, difficulty: DifficultyLevel.Easy);   // ❌ language uyumsuz

            // Act
            var result = await _sut.FilterAsync(ProgrammingLanguage.CSharp, DifficultyLevel.Easy, type: null);

            // Assert
            Assert.Single(result);
            Assert.Equal("C#", result[0].CategoryName);
            Assert.Equal("Easy", result[0].DifficultyLevel);
        }

        [Fact]
        public async Task FilterAsync_AllFilters_ReturnsOnlyExactMatch()
        {
            // Arrange
            var csharp = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "C#", ProgrammingLanguage.CSharp);
            var python = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "Python", ProgrammingLanguage.Python);

            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, csharp.Id, difficulty: DifficultyLevel.Medium, type: QuestionType.MultipleChoice); // ✅
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, csharp.Id, difficulty: DifficultyLevel.Hard, type: QuestionType.MultipleChoice);   // ❌ difficulty
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, csharp.Id, difficulty: DifficultyLevel.Medium, type: QuestionType.TrueFalse);      // ❌ type
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, python.Id, difficulty: DifficultyLevel.Medium, type: QuestionType.MultipleChoice); // ❌ language

            // Act
            var result = await _sut.FilterAsync(ProgrammingLanguage.CSharp, DifficultyLevel.Medium, QuestionType.MultipleChoice);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task FilterAsync_NoMatch_ReturnsEmptyList()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context, "C#", ProgrammingLanguage.CSharp);
            await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, difficulty: DifficultyLevel.Easy);

            // Act — Hard arıyoruz, hiç yok
            var result = await _sut.FilterAsync(language: null, difficulty: DifficultyLevel.Hard, type: null);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsQuestionDto()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            var dto = QuestionServiceTestHelpers.BuildCreateDto(category.Id, "Yeni soru");

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Yeni soru", result.QuestionText);
            Assert.Equal(category.Name, result.CategoryName);
        }

        [Fact]
        public async Task CreateAsync_ValidDto_PersistsQuestionToDatabase()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            var dto = QuestionServiceTestHelpers.BuildCreateDto(category.Id);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            var saved = await _context.Questions.FindAsync(result.Id);
            Assert.NotNull(saved);
            Assert.Equal(dto.QuestionText, saved.QuestionText);
        }

        [Fact]
        public async Task CreateAsync_ValidDto_PersistsOptionsToDatabase()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            var dto = QuestionServiceTestHelpers.BuildCreateDto(category.Id);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            var options = await _context.Options.Where(o => o.QuestionId == result.Id).ToListAsync();
            Assert.Equal(2, options.Count);
            Assert.Contains(options, o => o.IsCorrect);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingQuestion_ReturnsTrueAndDeletes()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            var question = await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id);

            // Act
            var result = await _sut.DeleteAsync(question.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.Questions.FindAsync(question.Id));
        }

        [Fact]
        public async Task DeleteAsync_QuestionNotFound_ReturnsFalse()
        {
            // Act
            var result = await _sut.DeleteAsync(id: 999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_OnlyDeletesTargetQuestion()
        {
            // Arrange
            var category = await QuestionServiceTestHelpers.SeedCategoryAsync(_context);
            var target = await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, "Silinecek");
            var other = await QuestionServiceTestHelpers.SeedQuestionAsync(_context, category.Id, "Kalacak");

            // Act
            await _sut.DeleteAsync(target.Id);

            // Assert
            Assert.Null(await _context.Questions.FindAsync(target.Id));
            Assert.NotNull(await _context.Questions.FindAsync(other.Id));
        }

        #endregion
    }
}