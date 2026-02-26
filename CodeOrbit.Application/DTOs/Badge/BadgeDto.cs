using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Badge
{
    public class BadgeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsEarned { get; set; }
        public DateTime? EarnedAt { get; set; }
        public int Progress { get; set; } // Kullanıcının şu anki ilerlemesi
        public int RequiredCount { get; set; } // Gereken sayı
    }
}
