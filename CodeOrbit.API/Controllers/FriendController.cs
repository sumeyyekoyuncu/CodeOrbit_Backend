using CodeOrbit.Application.DTOs.Friend;
using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest(SendFriendRequestDto dto)
        {
            var result = await _friendService.SendFriendRequestAsync(dto);
            if (!result)
                return BadRequest("İstek gönderilemedi.");
            return Ok("Arkadaşlık isteği gönderildi.");
        }

        [HttpPost("respond/{userId}")]
        public async Task<IActionResult> RespondToRequest(int userId, RespondToRequestDto dto)
        {
            var result = await _friendService.RespondToRequestAsync(userId, dto);
            if (!result)
                return BadRequest("İşlem başarısız.");
            return Ok(dto.Accept ? "İstek kabul edildi." : "İstek reddedildi.");
        }

        [HttpGet("requests/{userId}")]
        public async Task<IActionResult> GetPendingRequests(int userId)
        {
            var result = await _friendService.GetPendingRequestsAsync(userId);
            return Ok(result);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFriends(int userId)
        {
            var result = await _friendService.GetFriendsAsync(userId);
            return Ok(result);
        }

        [HttpDelete("{userId}/{friendId}")]
        public async Task<IActionResult> RemoveFriend(int userId, int friendId)
        {
            var result = await _friendService.RemoveFriendAsync(userId, friendId);
            if (!result)
                return NotFound("Arkadaşlık bulunamadı.");
            return Ok("Arkadaş kaldırıldı.");
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm, [FromQuery] int currentUserId)
        {
            var result = await _friendService.SearchUsersAsync(searchTerm, currentUserId);
            return Ok(result);
        }
    }
}