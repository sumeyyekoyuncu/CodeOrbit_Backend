using CodeOrbit.Application.DTOs.Badge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface IBadgeService
    {
        Task<List<BadgeDto>> GetUserBadgesAsync(int userId);
        Task CheckAndAwardBadgesAsync(int userId);
    }
}
