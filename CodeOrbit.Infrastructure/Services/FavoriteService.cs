using CodeOrbit.Application.DTOs.Favorite;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly AppDbContext _context;

        public FavoriteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddToFavoritesAsync(AddFavoriteDto dto)
        {
            // Zaten favorilerde mi kontrol et
            var exists = await _context.FavoriteQuestions
                .AnyAsync(f => f.UserId == dto.UserId && f.QuestionId == dto.QuestionId);

            if (exists)
                return false; // Zaten favorilerde

            var favorite = new FavoriteQuestion
            {
                UserId = dto.UserId,
                QuestionId = dto.QuestionId,
                AddedAt = DateTime.UtcNow
            };

            _context.FavoriteQuestions.Add(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromFavoritesAsync(int userId, int questionId)
        {
            var favorite = await _context.FavoriteQuestions
                .FirstOrDefaultAsync(f => f.UserId == userId && f.QuestionId == questionId);

            if (favorite == null)
                return false;

            _context.FavoriteQuestions.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FavoriteQuestionDto>> GetUserFavoritesAsync(int userId)
        {
            return await _context.FavoriteQuestions
                .Where(f => f.UserId == userId)
                .Include(f => f.Question)
                    .ThenInclude(q => q.Category)
                .OrderByDescending(f => f.AddedAt)
                .Select(f => new FavoriteQuestionDto
                {
                    Id = f.Id,
                    QuestionId = f.QuestionId,
                    QuestionText = f.Question.QuestionText,
                    CategoryName = f.Question.Category.Name,
                    DifficultyLevel = f.Question.DifficultyLevel.ToString(),
                    QuestionType = f.Question.QuestionType.ToString(),
                    AddedAt = f.AddedAt
                })
                .ToListAsync();
        }

        public async Task<bool> IsFavoriteAsync(int userId, int questionId)
        {
            return await _context.FavoriteQuestions
                .AnyAsync(f => f.UserId == userId && f.QuestionId == questionId);
        }
    }
}