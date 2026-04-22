using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.User
{
    public class UpdateAvatarDto
    {
        public int UserId { get; set; }
        public string Avatar { get; set; } = string.Empty;
    }
}
