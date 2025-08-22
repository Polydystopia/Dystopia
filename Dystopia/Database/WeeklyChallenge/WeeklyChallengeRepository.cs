using Microsoft.EntityFrameworkCore;
using Polytopia.Data;

namespace Dystopia.Database.WeeklyChallenge;

public class WeeklyChallengeRepository : IWeeklyChallengeRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public WeeklyChallengeRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeeklyChallengeEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.WeeklyChallenges.FindAsync(id);
    }

    public async Task<WeeklyChallengeEntity?> GetByWeekAsync(int week)
    {
        return await _dbContext.WeeklyChallenges
            .FirstOrDefaultAsync(wc => wc.Week == week);
    }

    public async Task<WeeklyChallengeEntity?> GetCurrentAsync()
    {
        return await _dbContext.WeeklyChallenges
            .OrderByDescending(wc => wc.Week)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WeeklyChallengeEntity>> GetByTribeAsync(TribeData.Type tribe)
    {
        return await _dbContext.WeeklyChallenges
            .Where(wc => wc.Tribe == tribe)
            .OrderByDescending(wc => wc.Week)
            .ToListAsync();
    }

    public async Task<List<WeeklyChallengeEntity>> GetAllAsync()
    {
        return await _dbContext.WeeklyChallenges
            .OrderByDescending(wc => wc.Week)
            .ToListAsync();
    }

    public async Task<List<WeeklyChallengeEntity>> GetRecentAsync(int count = 10)
    {
        return await _dbContext.WeeklyChallenges
            .OrderByDescending(wc => wc.Week)
            .Take(count)
            .ToListAsync();
    }

    public async Task<WeeklyChallengeEntity> CreateAsync(WeeklyChallengeEntity weeklyChallenge)
    {
        await _dbContext.WeeklyChallenges.AddAsync(weeklyChallenge);
        await _dbContext.SaveChangesAsync();
        return weeklyChallenge;
    }

    public async Task<bool> UpdateAsync(WeeklyChallengeEntity weeklyChallenge)
    {
        _dbContext.WeeklyChallenges.Update(weeklyChallenge);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var weeklyChallenge = await GetByIdAsync(id);
        if (weeklyChallenge == null)
            return false;

        _dbContext.WeeklyChallenges.Remove(weeklyChallenge);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}