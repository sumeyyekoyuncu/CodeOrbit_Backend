using CodeOrbit.Application.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Quiz
{
    public class QuizQuestionDto
    {
        public int QuizQuestionId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public List<OptionDto> Options { get; set; } = new();
    }
}
