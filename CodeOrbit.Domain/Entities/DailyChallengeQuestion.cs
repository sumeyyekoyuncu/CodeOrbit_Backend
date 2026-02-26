using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class DailyChallengeQuestion
    {
        public int Id { get; set; }
        public int DailyChallengeId { get; set; }
        public int QuestionId { get; set; }
        public int OrderNumber { get; set; } // Soruların sırası

        public DailyChallenge DailyChallenge { get; set; } = null!;
        public Question Question { get; set; } = null!;
    }
}
