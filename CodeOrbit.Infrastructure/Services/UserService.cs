using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Application.DTOs.User;
using CodeOrbit.Application.Interfaces;
using CodeOrbit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePhoto = user.ProfilePhoto,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UpdateUsernameAsync(UpdateUsernameDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return false;

            // Username başkası tarafından kullanılıyor mu?
            var exists = await _context.Users
                .AnyAsync(u => u.Username == dto.NewUsername && u.Id != dto.UserId);
            if (exists) return false;

            user.Username = dto.NewUsername;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePasswordAsync(UpdatePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return false;

            // Mevcut şifreyi kontrol et
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProfilePhotoAsync(UpdateProfilePhotoDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return false;

            user.ProfilePhoto = dto.PhotoBase64;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateAvatarAsync(UpdateAvatarDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return false;
            user.Avatar = dto.Avatar;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
