using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Friend
{
    public class RespondToRequestDto
    {
        public int RequestId { get; set; }
        public bool Accept { get; set; }
    }
}
