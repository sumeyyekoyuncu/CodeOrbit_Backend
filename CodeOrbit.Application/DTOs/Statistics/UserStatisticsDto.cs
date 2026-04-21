using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Statistics
{
    public class UserStatisticsDto
    {
        public int TotalQuizzes { get; set; }
        public int TotalQuestionsSolved { get; set; }
        public int TotalCorrectAnswers { get; set; }
        public int TotalWrongAnswers { get; set; }
        public double OverallSuccessRate { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public List<CategoryStatDto> CategoryStats { get; set; } = new();
        public List<DifficultyStatDto> DifficultyStats { get; set; } = new();
        public List<MostWrongQuestionDto> MostWrongQuestions { get; set; } = new();
    }
}
