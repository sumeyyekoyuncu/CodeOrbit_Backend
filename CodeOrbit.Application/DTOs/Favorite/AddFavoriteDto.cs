using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Favorite
{
    public class AddFavoriteDto
    {
        public int UserId { get; set; }
        public int QuestionId { get; set; }
    }
}
