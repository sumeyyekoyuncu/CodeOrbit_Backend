using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Challenge
{
    public class ChallengeLeaderboardDto
    {
        public int Rank { get; set; }
        public string Username { get; set; } = string.Empty;
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double SuccessRate { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
