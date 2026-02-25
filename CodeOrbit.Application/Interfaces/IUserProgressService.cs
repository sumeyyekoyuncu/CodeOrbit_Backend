using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Application.DTOs.UserProgress;

namespace CodeOrbit.Application.Interfaces
{
    public interface IUserProgressService
    {
        Task<bool> SubmitAnswerAsync(SubmitAnswerDto dto);
        Task<List<UserProgressDto>> GetUserProgressAsync(int userId);
    }
}
