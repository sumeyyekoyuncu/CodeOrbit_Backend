using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Challenge
{
    public class DailyChallengeDto
    {
        public int ChallengeId { get; set; }
        public DateTime Date { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public bool HasCompleted { get; set; }
        public List<ChallengeQuestionDto> Questions { get; set; } = new();
    }
}
