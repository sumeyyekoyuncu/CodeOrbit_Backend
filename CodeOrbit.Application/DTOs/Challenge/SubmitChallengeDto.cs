using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Challenge
{
    public class SubmitChallengeDto
    {
        public int UserId { get; set; }
        public int DailyChallengeId { get; set; }
        public List<ChallengeAnswerDto> Answers { get; set; } = new();
    }
}
