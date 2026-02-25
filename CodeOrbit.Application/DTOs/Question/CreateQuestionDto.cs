using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Domain.Enums;

namespace CodeOrbit.Application.DTOs.Question
{
    public class CreateQuestionDto
    {
        public int CategoryId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QuestionType QuestionType { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public List<CreateOptionDto> Options { get; set; } = new();
    }

}
