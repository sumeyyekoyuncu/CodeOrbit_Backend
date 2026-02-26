using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Quiz
{
    public class SubmitQuizAnswerDto
    {
        public int QuizId { get; set; }
        public int QuizQuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}
