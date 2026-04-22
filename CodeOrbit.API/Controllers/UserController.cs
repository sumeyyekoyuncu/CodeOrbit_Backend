using CodeOrbit.Application.DTOs.User;
using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPut("username")]
        public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameDto dto)
        {
            var result = await _userService.UpdateUsernameAsync(dto);
            if (!result) return BadRequest("Kullanıcı adı zaten kullanımda veya kullanıcı bulunamadı.");
            return Ok();
        }

        [HttpPut("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            var result = await _userService.UpdatePasswordAsync(dto);
            if (!result) return BadRequest("Mevcut şifre yanlış veya kullanıcı bulunamadı.");
            return Ok();
        }

        [HttpPut("profile-photo")]
        public async Task<IActionResult> UpdateProfilePhoto([FromBody] UpdateProfilePhotoDto dto)
        {
            var result = await _userService.UpdateProfilePhotoAsync(dto);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
