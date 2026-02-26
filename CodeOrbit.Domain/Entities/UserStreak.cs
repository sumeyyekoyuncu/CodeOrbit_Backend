using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class UserStreak
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime LastActiveDate { get; set; } // Sadece tarih

        public User User { get; set; } = null!;
    }
}
