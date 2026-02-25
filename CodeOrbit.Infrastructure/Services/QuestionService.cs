using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Application.DTOs.Question;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly AppDbContext _context;

        public QuestionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuestionDto>> GetAllAsync()
        {
            return await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Options)
                .Select(q => ToDto(q))
                .ToListAsync();
        }

        public async Task<List<QuestionDto>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Questions
                .Where(q => q.CategoryId == categoryId)
                .Include(q => q.Category)
                .Include(q => q.Options)
                .Select(q => ToDto(q))
                .ToListAsync();
        }

        public async Task<List<QuestionDto>> FilterAsync(ProgrammingLanguage? language, DifficultyLevel? difficulty, QuestionType? type)
        {
            var query = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Options)
                .AsQueryable();

            if (language.HasValue)
                query = query.Where(q => q.Category.Language == language.Value);

            if (difficulty.HasValue)
                query = query.Where(q => q.DifficultyLevel == difficulty.Value);

            if (type.HasValue)
                query = query.Where(q => q.QuestionType == type.Value);

            return await query.Select(q => ToDto(q)).ToListAsync();
        }

        public async Task<QuestionDto?> GetByIdAsync(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            return question == null ? null : ToDto(question);
        }

        public async Task<QuestionDto> CreateAsync(CreateQuestionDto dto)
        {
            var question = new Question
            {
                CategoryId = dto.CategoryId,
                QuestionText = dto.QuestionText,
                QuestionType = dto.QuestionType,
                DifficultyLevel = dto.DifficultyLevel,
                Options = dto.Options.Select(o => new Option
                {
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return ToDto(question);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null) return false;

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        private static QuestionDto ToDto(Question q) => new QuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType.ToString(),
            DifficultyLevel = q.DifficultyLevel.ToString(),
            CategoryName = q.Category?.Name ?? string.Empty,
            Options = q.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                OptionText = o.OptionText
            }).ToList()
        };
    }
}
