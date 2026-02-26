using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public int? UserAnswerOptionId { get; set; }
        public bool? IsCorrect { get; set; }

        public Quiz Quiz { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public Option? UserAnswerOption { get; set; }
    }
}
