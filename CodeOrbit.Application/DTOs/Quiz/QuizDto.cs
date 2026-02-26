using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Quiz
{
    public class QuizDto
    {
        public int QuizId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }
}
