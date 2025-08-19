namespace Dystopia.Database.WeeklyChallenge.League;

public interface ILeagueRepository
{
    Task<LeagueEntity?> GetByIdAsync(int id);
    Task<LeagueEntity?> GetByNameAsync(string name);
    Task<List<LeagueEntity>> GetAllAsync();
    Task<LeagueEntity?> GetFriendsLeagueAsync();
    Task<List<LeagueEntity>> GetCompetitiveLeaguesAsync();
    Task<LeagueEntity?> GetEntryLeagueAsync();
    Task<LeagueEntity> CreateAsync(LeagueEntity league);
    Task<bool> UpdateAsync(LeagueEntity league);
    Task<bool> DeleteAsync(int id);
}