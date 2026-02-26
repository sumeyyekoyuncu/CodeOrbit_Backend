using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Statistics
{
    public class CategoryStatDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int QuestionsSolved { get; set; }
        public int CorrectAnswers { get; set; }
        public double SuccessRate { get; set; }
    }
}
