using CodeOrbit.Application.DTOs.Question;

namespace CodeOrbit.Application.Interfaces
{
    public interface IAiQuestionService
    {
        Task<List<QuestionDto>> GenerateQuestionsAsync(GenerateQuestionRequestDto request);
    }
}