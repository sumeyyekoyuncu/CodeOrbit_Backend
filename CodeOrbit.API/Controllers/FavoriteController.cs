using CodeOrbit.Application.DTOs.Favorite;
using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites(AddFavoriteDto dto)
        {
            var result = await _favoriteService.AddToFavoritesAsync(dto);
            if (!result)
                return BadRequest("Bu soru zaten favorilerde.");
            return Ok("Favorilere eklendi.");
        }

        [HttpDelete("{userId}/{questionId}")]
        public async Task<IActionResult> RemoveFromFavorites(int userId, int questionId)
        {
            var result = await _favoriteService.RemoveFromFavoritesAsync(userId, questionId);
            if (!result)
                return NotFound("Favori bulunamadı.");
            return Ok("Favorilerden kaldırıldı.");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            var result = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(result);
        }

        [HttpGet("{userId}/check/{questionId}")]
        public async Task<IActionResult> IsFavorite(int userId, int questionId)
        {
            var result = await _favoriteService.IsFavoriteAsync(userId, questionId);
            return Ok(new { isFavorite = result });
        }
    }
}