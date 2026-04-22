using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Application.DTOs.User;

namespace CodeOrbit.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<bool> UpdateUsernameAsync(UpdateUsernameDto dto);
        Task<bool> UpdatePasswordAsync(UpdatePasswordDto dto);
        Task<bool> UpdateProfilePhotoAsync(UpdateProfilePhotoDto dto);
    }
}
