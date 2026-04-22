using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.User
{
    public class UpdateUsernameDto
    {
        public int UserId { get; set; }
        public string NewUsername { get; set; } = string.Empty;
    }
}
