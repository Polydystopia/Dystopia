using Microsoft.EntityFrameworkCore;

namespace Dystopia.Database.WeeklyChallenge;

public class WeeklyChallengeEntryRepository : IWeeklyChallengeEntryRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public WeeklyChallengeEntryRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeeklyChallengeEntryEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.WeeklyChallengeEntries
            .Include(e => e.User)
            .Include(e => e.WeeklyChallenge)
            .Include(e => e.League)
            .Include(e => e.Game)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<WeeklyChallengeEntryEntity>> GetByUserAndChallengeAsync(Guid userId, int weeklyChallengeId)
    {
        return await _dbContext.WeeklyChallengeEntries
            .Include(e => e.User)
            .Include(e => e.WeeklyChallenge)
            .Include(e => e.League)
            .Include(e => e.Game)
            .Where(e => e.UserId == userId && e.WeeklyChallengeId == weeklyChallengeId).ToListAsync();
    }

    public async Task<List<WeeklyChallengeEntryEntity>> GetByUserAsync(Guid userId)
    {
        return await _dbContext.WeeklyChallengeEntries
            .Include(e => e.WeeklyChallenge)
            .Include(e => e.League)
            .Include(e => e.Game)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DateCreated)
            .ToListAsync();
    }

    public async Task<List<WeeklyChallengeEntryEntity>> GetByChallengeAsync(int weeklyChallengeId)
    {
        return await _dbContext.WeeklyChallengeEntries
            .Include(e => e.User)
            .Include(e => e.League)
            .Include(e => e.Game)
            .Where(e => e.WeeklyChallengeId == weeklyChallengeId)
            .OrderByDescending(e => e.Score)
            .ToListAsync();
    }

    public async Task<List<WeeklyChallengeEntryEntity>> GetLeaderboardAsync(int weeklyChallengeId, int leagueId,
        int limit = 100)
    {
        return await _dbContext.WeeklyChallengeEntries
            .Include(e => e.User)
            .Include(e => e.Game)
            .Where(e => e.WeeklyChallengeId == weeklyChallengeId && e.LeagueId == leagueId && e.IsValid)
            .OrderByDescending(e => e.Score)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<WeeklyChallengeEntryEntity>> GetByUserAndLeagueAsync(Guid userId, int leagueId)
    {
        return await _dbContext.WeeklyChallengeEntries
            .Include(e => e.WeeklyChallenge)
            .Include(e => e.Game)
            .Where(e => e.UserId == userId && e.LeagueId == leagueId)
            .OrderByDescending(e => e.DateCreated)
            .ToListAsync();
    }

    public async Task<List<WeeklyChallengeEntryEntity>> GetByChallengeAndDayAsync(int weeklyChallengeId, int day)
    {
        return await _dbContext.WeeklyChallengeEntries
            .Include(e => e.User)
            .Include(e => e.League)
            .Include(e => e.Game)
            .Where(e => e.WeeklyChallengeId == weeklyChallengeId && e.Day == day)
            .OrderByDescending(e => e.Score)
            .ToListAsync();
    }

    public async Task<WeeklyChallengeEntryEntity?> GetByUserChallengeAndDayAsync(Guid userId, int weeklyChallengeId,
        int day)
    {
        return await _dbContext.WeeklyChallengeEntries
            .Include(e => e.User)
            .Include(e => e.WeeklyChallenge)
            .Include(e => e.League)
            .Include(e => e.Game)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.WeeklyChallengeId == weeklyChallengeId && e.Day == day);
    }

    public async Task<WeeklyChallengeEntryEntity> SaveOrUpdateAsync(WeeklyChallengeEntryEntity entry)
    {
        var existing = await GetByUserChallengeAndDayAsync(entry.UserId, entry.WeeklyChallengeId, entry.Day);

        if (existing == null)
        {
            await _dbContext.WeeklyChallengeEntries.AddAsync(entry);
        }
        else
        {
            existing.Day = entry.Day;
            existing.Score = entry.Score;
            existing.HasFinished = entry.HasFinished;
            existing.HasReplay = entry.HasReplay;
            existing.IsValid = entry.IsValid;
            existing.GameId = entry.GameId;
            existing.DateCreated = entry.DateCreated;
            entry = existing;
        }

        await _dbContext.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _dbContext.WeeklyChallengeEntries.FindAsync(id);
        if (entry == null)
            return false;

        _dbContext.WeeklyChallengeEntries.Remove(entry);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUserRankAsync(Guid userId, int weeklyChallengeId, int leagueId)
    {
        var userEntry = await _dbContext.WeeklyChallengeEntries
            .FirstOrDefaultAsync(e =>
                e.UserId == userId && e.WeeklyChallengeId == weeklyChallengeId && e.LeagueId == leagueId);

        if (userEntry == null || !userEntry.IsValid)
            return -1;

        var rank = await _dbContext.WeeklyChallengeEntries
            .Where(e => e.WeeklyChallengeId == weeklyChallengeId && e.LeagueId == leagueId && e.IsValid &&
                        e.Score > userEntry.Score)
            .CountAsync();

        return rank + 1;
    }
}