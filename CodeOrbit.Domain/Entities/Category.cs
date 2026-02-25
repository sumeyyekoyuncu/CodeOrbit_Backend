using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Domain.Enums;

namespace CodeOrbit.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ProgrammingLanguage Language { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
