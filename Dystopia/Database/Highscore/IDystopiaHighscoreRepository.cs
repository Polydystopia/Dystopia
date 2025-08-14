namespace Dystopia.Database.Highscore;

public interface IDystopiaHighscoreRepository
{
    Task<HighscoreEntity?> GetByUserAndTribeAsync(Guid userId, int tribe);
    Task<IEnumerable<HighscoreEntity>> GetByTribeAsync(int tribe, int limit = 100);
    Task<IEnumerable<HighscoreEntity>> GetByUserAsync(Guid userId);
    Task SaveOrUpdateAsync(HighscoreEntity highscore);
    Task DeleteAsync(Guid userId, int tribe);
}