using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Domain.Enums;
using Microsoft.VisualBasic.FileIO;

namespace CodeOrbit.Domain.Entities
{
    public class Question
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QuestionType QuestionType { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }

        public Category Category { get; set; } = null!;
        public ICollection<Option> Options { get; set; } = new List<Option>();
        public ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();
    }
}
