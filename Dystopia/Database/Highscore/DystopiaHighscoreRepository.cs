using Microsoft.EntityFrameworkCore;

namespace Dystopia.Database.Highscore;

public class DystopiaHighscoreRepository : IDystopiaHighscoreRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public DystopiaHighscoreRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HighscoreEntity?> GetByUserAndTribeAsync(Guid userId, int tribe)
    {
        return await _dbContext.Highscores
            .Include(h => h.User)
            .FirstOrDefaultAsync(h => h.UserId == userId && h.Tribe == tribe);
    }

    public async Task<IEnumerable<HighscoreEntity>> GetByTribeAsync(int tribe, int limit = 100)
    {
        return await _dbContext.Highscores
            .Include(h => h.User)
            .Where(h => h.Tribe == tribe)
            .OrderByDescending(h => h.Score)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<HighscoreEntity>> GetByUserAsync(Guid userId)
    {
        return await _dbContext.Highscores
            .Include(h => h.User)
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Score)
            .ToListAsync();
    }

    public async Task SaveOrUpdateAsync(HighscoreEntity highscore)
    {
        var existing = await _dbContext.Highscores
            .FirstOrDefaultAsync(h => h.UserId == highscore.UserId && h.Tribe == highscore.Tribe);

        if (existing != null)
        {
            if (highscore.Score > existing.Score)
            {
                existing.Score = highscore.Score;
                existing.FinalGameStateData = highscore.FinalGameStateData;
                await _dbContext.SaveChangesAsync();
            }
        }
        else
        {
            _dbContext.Highscores.Add(highscore);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid userId, int tribe)
    {
        var highscore = await _dbContext.Highscores
            .FirstOrDefaultAsync(h => h.UserId == userId && h.Tribe == tribe);

        if (highscore != null)
        {
            _dbContext.Highscores.Remove(highscore);
            await _dbContext.SaveChangesAsync();
        }
    }
}