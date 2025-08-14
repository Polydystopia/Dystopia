using Polytopia.Data;

namespace Dystopia.Database.Highscore;

public interface IDystopiaHighscoreRepository
{
    Task<HighscoreEntity?> GetByUserAndTribeAsync(Guid userId, TribeData.Type tribe);
    Task<IEnumerable<HighscoreEntity>> GetByTribeAsync(TribeData.Type tribe, int limit = 100);
    Task<IEnumerable<HighscoreEntity>> GetAsync(int limit = 100);
    Task<IEnumerable<HighscoreEntity>> GetByUserAsync(Guid userId);
    Task SaveOrUpdateAsync(HighscoreEntity highscore);
    Task DeleteAsync(Guid userId, TribeData.Type tribe);
}