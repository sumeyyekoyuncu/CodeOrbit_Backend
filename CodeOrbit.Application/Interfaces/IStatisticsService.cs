using CodeOrbit.Application.DTOs.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface IStatisticsService
    {
        Task<UserStatisticsDto> GetUserStatisticsAsync(int userId);
    }
}
