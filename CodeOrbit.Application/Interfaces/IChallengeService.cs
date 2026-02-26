using CodeOrbit.Application.DTOs.Challenge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Application.Interfaces
{
    public interface IChallengeService
    {
        Task<DailyChallengeDto> GetTodaysChallengeAsync(int userId);
        Task<ChallengeResultDto> SubmitChallengeAsync(SubmitChallengeDto dto);
        Task<List<ChallengeLeaderboardDto>> GetTodaysLeaderboardAsync();
        Task GenerateDailyChallengeAsync(); // Admin/Background job için
    }
}
