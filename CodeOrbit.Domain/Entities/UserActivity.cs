using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class UserActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; } // Sadece tarih (saat olmadan)
        public int QuestionsSolved { get; set; }
        public DateTime LastActivityAt { get; set; }

        public User User { get; set; } = null!;
    }
}
