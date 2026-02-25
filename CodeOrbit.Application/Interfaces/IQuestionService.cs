using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Application.DTOs.Question;
using CodeOrbit.Domain.Enums;

namespace CodeOrbit.Application.Interfaces
{
    public interface IQuestionService
    {
        Task<List<QuestionDto>> GetAllAsync();
        Task<List<QuestionDto>> GetByCategoryAsync(int categoryId);
        Task<List<QuestionDto>> FilterAsync(ProgrammingLanguage? language, DifficultyLevel? difficulty, QuestionType? type);
        Task<QuestionDto?> GetByIdAsync(int id);
        Task<QuestionDto> CreateAsync(CreateQuestionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
