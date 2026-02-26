using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.Friend
{
    public class SendFriendRequestDto
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
    }
}
