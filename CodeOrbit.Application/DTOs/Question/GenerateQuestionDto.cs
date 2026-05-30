namespace CodeOrbit.Application.DTOs.Question
{
    public class GenerateQuestionRequestDto
    {
        public string CategoryName { get; set; } = "";
        public string DifficultyLevel { get; set; } = "Medium";
        public string QuestionType { get; set; } = "MultipleChoice";
        public int Count { get; set; } = 5;
        public int CategoryId { get; set; }
    }
}