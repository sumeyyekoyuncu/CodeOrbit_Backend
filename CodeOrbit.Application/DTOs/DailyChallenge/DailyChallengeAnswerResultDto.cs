using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.DailyChallenge
{

    public class DailyChallengeAnswerResultDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public int CorrectOptionId { get; set; } // Doğru cevap hangisiydi
    }
}
