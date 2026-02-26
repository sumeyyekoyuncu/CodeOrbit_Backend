using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Leaderboard
{
    public class CategoryLeaderboardDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<LeaderboardEntryDto> Entries { get; set; } = new();
    }
}
