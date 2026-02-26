using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Leaderboard
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Score { get; set; } // Puan (quiz sayısı, doğru cevap vs.)
        public double SuccessRate { get; set; }
        public int BadgeCount { get; set; }
        public int CurrentStreak { get; set; }
        public bool IsCurrentUser { get; set; } // Kullanıcının kendisi mi?
    }
}
