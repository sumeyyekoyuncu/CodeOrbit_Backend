using CodeOrbit.Application.DTOs.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface IQuizService
    {
        Task<QuizDto> StartQuizAsync(StartQuizDto dto);
        Task<bool> SubmitAnswerAsync(SubmitQuizAnswerDto dto);
        Task<QuizResultDto> CompleteQuizAsync(int quizId);
        Task<List<QuizHistoryDto>> GetUserQuizHistoryAsync(int userId);
    }
}
