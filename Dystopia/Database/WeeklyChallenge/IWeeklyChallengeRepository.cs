using Polytopia.Data;

namespace Dystopia.Database.WeeklyChallenge;

public interface IWeeklyChallengeRepository
{
    Task<WeeklyChallengeEntity?> GetByIdAsync(int id);
    Task<WeeklyChallengeEntity?> GetByWeekAsync(int week);
    Task<WeeklyChallengeEntity?> GetCurrentAsync();
    Task<List<WeeklyChallengeEntity>> GetByTribeAsync(TribeData.Type tribe);
    Task<List<WeeklyChallengeEntity>> GetAllAsync();
    Task<List<WeeklyChallengeEntity>> GetRecentAsync(int count = 10);
    Task<WeeklyChallengeEntity> CreateAsync(WeeklyChallengeEntity weeklyChallenge);
    Task<bool> UpdateAsync(WeeklyChallengeEntity weeklyChallenge);
    Task<bool> DeleteAsync(int id);
}