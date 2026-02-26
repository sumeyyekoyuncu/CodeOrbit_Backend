using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Activity
{
    public class UserActivityDto
    {
        public int TodayQuestionsSolved { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime LastActiveDate { get; set; }
        public List<DailyActivityDto> Last7Days { get; set; } = new();
    }
}
