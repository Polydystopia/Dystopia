namespace Dystopia.Database.WeeklyChallenge;

public interface IWeeklyChallengeEntryRepository
{
    Task<WeeklyChallengeEntryEntity?> GetByIdAsync(int id);
    Task<List<WeeklyChallengeEntryEntity>> GetByUserAndChallengeAsync(Guid userId, int weeklyChallengeId);
    Task<List<WeeklyChallengeEntryEntity>> GetByUserAsync(Guid userId);
    Task<List<WeeklyChallengeEntryEntity>> GetByChallengeAsync(int weeklyChallengeId);
    Task<List<WeeklyChallengeEntryEntity>> GetLeaderboardAsync(int weeklyChallengeId, int leagueId, int limit = 100);
    Task<List<WeeklyChallengeEntryEntity>> GetByUserAndLeagueAsync(Guid userId, int leagueId);
    Task<List<WeeklyChallengeEntryEntity>> GetByChallengeAndDayAsync(int weeklyChallengeId, int day);
    Task<WeeklyChallengeEntryEntity?> GetByUserChallengeAndDayAsync(Guid userId, int weeklyChallengeId, int day);
    Task<WeeklyChallengeEntryEntity> SaveOrUpdateAsync(WeeklyChallengeEntryEntity entry);
    Task<bool> DeleteAsync(int id);
    Task<int> GetUserRankAsync(Guid userId, int weeklyChallengeId, int leagueId);
    Task<List<WeeklyChallengeEntryEntity>> GetBestEntriesPerUserByLeagueAsync(int weeklyChallengeId, int leagueId);
    Task<List<WeeklyChallengeEntryEntity>> GetBestEntriesPerUserAsync(int weeklyChallengeId);
}