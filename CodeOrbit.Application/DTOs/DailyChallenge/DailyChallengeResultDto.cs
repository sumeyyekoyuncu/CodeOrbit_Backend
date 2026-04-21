using System.Collections.Generic;
using CodeOrbit.Application.DTOs.DailyChallenge;

namespace CodeOrbit.Application.DTOs.Challenge
{
    public class DailyChallengeResultDto
    {
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double SuccessRate { get; set; }
        public int Rank { get; set; }
        public int TotalParticipants { get; set; }
        public List<DailyChallengeAnswerResultDto> AnswerResults { get; set; } = new();
    }
}