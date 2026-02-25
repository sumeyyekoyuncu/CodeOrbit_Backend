using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.UserProgress
{
    public class SubmitAnswerDto
    {
        public int UserId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}
