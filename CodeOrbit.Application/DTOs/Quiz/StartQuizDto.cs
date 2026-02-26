using CodeOrbit.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Quiz
{
    public class StartQuizDto
    {
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public int QuestionCount { get; set; } = 10;
    }
}
