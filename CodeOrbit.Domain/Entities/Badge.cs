using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class Badge
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty; // Emoji veya icon adı
        public string Requirement { get; set; } = string.Empty; // "Complete 10 quizzes"
        public int RequiredCount { get; set; } // Sayısal gereksinim

        public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    }
}
