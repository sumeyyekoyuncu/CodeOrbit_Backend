using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class UserChallengeAnswer
    {
        public int Id { get; set; }
        public int UserChallengeAttemptId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
        public bool IsCorrect { get; set; }

        public UserChallengeAttempt Attempt { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public Option SelectedOption { get; set; } = null!;
    }
}
