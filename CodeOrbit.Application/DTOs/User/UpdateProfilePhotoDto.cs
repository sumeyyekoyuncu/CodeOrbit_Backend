using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.DTOs.User
{
    public class UpdateProfilePhotoDto
    {
        public int UserId { get; set; }
        public string PhotoBase64 { get; set; } = string.Empty;
    }
}
