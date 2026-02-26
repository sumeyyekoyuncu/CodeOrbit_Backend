using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Domain.Entities
{
    public class Friendship
    {
        public int Id { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User1 { get; set; } = null!;
        public User User2 { get; set; } = null!;
    }
}
