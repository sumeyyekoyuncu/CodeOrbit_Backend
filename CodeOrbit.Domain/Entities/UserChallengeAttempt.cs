using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class UserChallengeAttempt
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DailyChallengeId { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedAt { get; set; }

        public User User { get; set; } = null!;
        public DailyChallenge DailyChallenge { get; set; } = null!;
        public ICollection<UserChallengeAnswer> Answers { get; set; } = new List<UserChallengeAnswer>();
    }
}
