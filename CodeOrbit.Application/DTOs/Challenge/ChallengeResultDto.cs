using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Challenge
{
    public class ChallengeResultDto
    {
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double SuccessRate { get; set; }
        public int Rank { get; set; } // Bugünkü challenge'da kaçıncı olduğu
        public int TotalParticipants { get; set; }
    }
}
