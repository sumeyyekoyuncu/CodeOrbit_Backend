using CodeOrbit.Application.DTOs.Favorite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<bool> AddToFavoritesAsync(AddFavoriteDto dto);
        Task<bool> RemoveFromFavoritesAsync(int userId, int questionId);
        Task<List<FavoriteQuestionDto>> GetUserFavoritesAsync(int userId);
        Task<bool> IsFavoriteAsync(int userId, int questionId);
    }
}
