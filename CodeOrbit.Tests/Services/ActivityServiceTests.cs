using CodeOrbit.Application.DTOs.Activity;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;
using CodeOrbit.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CodeOrbit.Tests.Services
{
    public class ActivityServiceTests : IDisposable
    {
        #region Setup & Teardown

        private readonly AppDbContext _context;
        private readonly ActivityService _sut;

        public ActivityServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new ActivityService(_context);
        }

        public void Dispose() => _context.Dispose();

        #endregion

        #region GetUserActivityAsync

        [Fact]
        public async Task GetUserActivityAsync_UserHasNoData_ReturnsZerosAndCreatesStreak()
        {
            // Act
            var result = await _sut.GetUserActivityAsync(userId: 1);

            // Assert
            Assert.Equal(0, result.TodayQuestionsSolved);
            Assert.Equal(0, result.CurrentStreak);
            Assert.Equal(0, result.LongestStreak);
            Assert.Empty(result.Last7Days);

            var streak = await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == 1);
            Assert.NotNull(streak);
        }

        [Fact]
        public async Task GetUserActivityAsync_UserHasTodayActivity_ReturnsTodayCount()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            _context.UserActivities.Add(new UserActivity
            {
                UserId = 1,
                Date = today,
                QuestionsSolved = 5,
                LastActivityAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserActivityAsync(userId: 1);

            // Assert
            Assert.Equal(5, result.TodayQuestionsSolved);
        }

        [Fact]
        public async Task GetUserActivityAsync_ReturnsOnlyLast7Days()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            for (int i = 0; i < 10; i++)
            {
                _context.UserActivities.Add(new UserActivity
                {
                    UserId = 1,
                    Date = today.AddDays(-i),
                    QuestionsSolved = i + 1,
                    LastActivityAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserActivityAsync(userId: 1);

            // Assert
            Assert.True(result.Last7Days.Count <= 8);
            Assert.All(result.Last7Days, d => Assert.True(d.Date >= today.AddDays(-7)));
        }

        [Fact]
        public async Task GetUserActivityAsync_ReturnsCorrectStreakData()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            _context.UserStreaks.Add(new UserStreak
            {
                UserId = 1,
                CurrentStreak = 5,
                LongestStreak = 10,
                LastActiveDate = today
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserActivityAsync(userId: 1);

            // Assert
            Assert.Equal(5, result.CurrentStreak);
            Assert.Equal(10, result.LongestStreak);
        }

        #endregion

        #region CanStartQuizAsync

        [Theory]
        [InlineData(1, 1)]
        [InlineData(99, 100)]
        [InlineData(0, 0)]
        public async Task CanStartQuizAsync_AlwaysReturnsTrue(int userId, int questionCount)
        {
            var result = await _sut.CanStartQuizAsync(userId, questionCount);
            Assert.True(result);
        }

        #endregion

        #region UpdateActivityAsync — Aktivite

        [Fact]
        public async Task UpdateActivityAsync_NewUser_CreatesActivityRecord()
        {
            // Act
            await _sut.UpdateActivityAsync(userId: 1, questionsSolved: 3);

            // Assert
            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(a => a.UserId == 1 && a.Date == DateTime.UtcNow.Date);

            Assert.NotNull(activity);
            Assert.Equal(3, activity.QuestionsSolved);
        }

        [Fact]
        public async Task UpdateActivityAsync_ExistingActivityToday_AccumulatesCount()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            _context.UserActivities.Add(new UserActivity
            {
                UserId = 1,
                Date = today,
                QuestionsSolved = 4,
                LastActivityAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateActivityAsync(userId: 1, questionsSolved: 6);

            // Assert
            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(a => a.UserId == 1 && a.Date == today);

            Assert.Equal(10, activity!.QuestionsSolved); // 4 + 6
        }

        #endregion

        #region UpdateActivityAsync — Streak

        [Fact]
        public async Task UpdateActivityAsync_FirstActivity_CreatesStreakWithValue1()
        {
            // Act
            await _sut.UpdateActivityAsync(userId: 1, questionsSolved: 1);

            // Assert
            var streak = await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == 1);
            Assert.NotNull(streak);
            Assert.Equal(1, streak.CurrentStreak);
            Assert.Equal(1, streak.LongestStreak);
        }

        [Fact]
        public async Task UpdateActivityAsync_ConsecutiveDays_IncrementsStreak()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            _context.UserStreaks.Add(new UserStreak
            {
                UserId = 1,
                CurrentStreak = 3,
                LongestStreak = 3,
                LastActiveDate = today.AddDays(-1)
            });
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateActivityAsync(userId: 1, questionsSolved: 2);

            // Assert
            var streak = await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == 1);
            Assert.Equal(4, streak!.CurrentStreak);
            Assert.Equal(4, streak.LongestStreak);
        }

        [Fact]
        public async Task UpdateActivityAsync_StreakBroken_ResetsToOne()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            _context.UserStreaks.Add(new UserStreak
            {
                UserId = 1,
                CurrentStreak = 7,
                LongestStreak = 7,
                LastActiveDate = today.AddDays(-2)
            });
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateActivityAsync(userId: 1, questionsSolved: 1);

            // Assert
            var streak = await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == 1);
            Assert.Equal(1, streak!.CurrentStreak);
            Assert.Equal(7, streak.LongestStreak); // En uzun etkilenmemeli
        }

        [Fact]
        public async Task UpdateActivityAsync_SameDayMultipleCalls_StreakNotDoubleIncremented()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            _context.UserStreaks.Add(new UserStreak
            {
                UserId = 1,
                CurrentStreak = 5,
                LongestStreak = 5,
                LastActiveDate = today
            });
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateActivityAsync(userId: 1, questionsSolved: 3);
            await _sut.UpdateActivityAsync(userId: 1, questionsSolved: 2);

            // Assert
            var streak = await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == 1);
            Assert.Equal(5, streak!.CurrentStreak);
        }

        [Fact]
        public async Task UpdateActivityAsync_NewConsecutiveRecord_UpdatesLongestStreak()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            _context.UserStreaks.Add(new UserStreak
            {
                UserId = 1,
                CurrentStreak = 10,
                LongestStreak = 10,
                LastActiveDate = today.AddDays(-1)
            });
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateActivityAsync(userId: 1, questionsSolved: 1);

            // Assert
            var streak = await _context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == 1);
            Assert.Equal(11, streak!.CurrentStreak);
            Assert.Equal(11, streak.LongestStreak);
        }

        #endregion
    }
}