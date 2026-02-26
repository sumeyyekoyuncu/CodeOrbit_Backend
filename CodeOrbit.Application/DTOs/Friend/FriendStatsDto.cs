using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Friend
{
    public class FriendStatsDto
    {
        public int TotalQuizzes { get; set; }
        public int CurrentStreak { get; set; }
        public double OverallSuccessRate { get; set; }
    }

}
