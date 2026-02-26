using CodeOrbit.Application.DTOs.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface IActivityService
    {
        Task<UserActivityDto> GetUserActivityAsync(int userId);
        Task<bool> CanStartQuizAsync(int userId, int questionCount);
        Task UpdateActivityAsync(int userId, int questionsSolved);
    }
}
