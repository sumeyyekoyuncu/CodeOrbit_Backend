using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Application.DTOs.UserProgress;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class UserProgressService : IUserProgressService
    {
        private readonly AppDbContext _context;

        public UserProgressService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SubmitAnswerAsync(SubmitAnswerDto dto)
        {
            var option = await _context.Options.FindAsync(dto.SelectedOptionId);
            if (option == null) return false;

            var progress = new UserProgress
            {
                UserId = dto.UserId,
                QuestionId = dto.QuestionId,
                IsCorrect = option.IsCorrect,
                AnsweredAt = DateTime.UtcNow
            };

            _context.UserProgresses.Add(progress);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserProgressDto>> GetUserProgressAsync(int userId)
        {
            return await _context.UserProgresses
                .Where(p => p.UserId == userId)
                .Include(p => p.Question)
                .Select(p => new UserProgressDto
                {
                    QuestionId = p.QuestionId,
                    QuestionText = p.Question.QuestionText,
                    IsCorrect = p.IsCorrect,
                    AnsweredAt = p.AnsweredAt
                })
                .ToListAsync();
        }
    }
}
