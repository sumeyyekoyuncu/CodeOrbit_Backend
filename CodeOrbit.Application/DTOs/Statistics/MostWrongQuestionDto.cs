using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Statistics
{
    public class MostWrongQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TimesAnswered { get; set; }
        public int TimesWrong { get; set; }
        public double WrongRate { get; set; }
    }
}
