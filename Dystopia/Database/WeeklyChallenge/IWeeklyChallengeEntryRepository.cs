namespace Dystopia.Database.WeeklyChallenge;

public interface IWeeklyChallengeEntryRepository
{
    Task<WeeklyChallengeEntryEntity?> GetByIdAsync(int id);
    Task<WeeklyChallengeEntryEntity?> GetByUserAndChallengeAsync(Guid userId, int weeklyChallengeId);
    Task<List<WeeklyChallengeEntryEntity>> GetByUserAsync(Guid userId);
    Task<List<WeeklyChallengeEntryEntity>> GetByChallengeAsync(int weeklyChallengeId);
    Task<List<WeeklyChallengeEntryEntity>> GetLeaderboardAsync(int weeklyChallengeId, int leagueId, int limit = 100);
    Task<List<WeeklyChallengeEntryEntity>> GetByUserAndLeagueAsync(Guid userId, int leagueId);
    Task<WeeklyChallengeEntryEntity> SaveOrUpdateAsync(WeeklyChallengeEntryEntity entry);
    Task<bool> DeleteAsync(int id);
    Task<int> GetUserRankAsync(Guid userId, int weeklyChallengeId, int leagueId);
}