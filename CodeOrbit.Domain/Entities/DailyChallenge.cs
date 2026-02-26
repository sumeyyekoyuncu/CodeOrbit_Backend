using CodeOrbit.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class DailyChallenge
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } // Sadece tarih
        public int CategoryId { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
        public ICollection<DailyChallengeQuestion> Questions { get; set; } = new List<DailyChallengeQuestion>();
        public ICollection<UserChallengeAttempt> UserAttempts { get; set; } = new List<UserChallengeAttempt>();
    }
}
