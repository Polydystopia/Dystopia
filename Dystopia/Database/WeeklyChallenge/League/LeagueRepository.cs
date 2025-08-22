using Microsoft.EntityFrameworkCore;

namespace Dystopia.Database.WeeklyChallenge.League;

public class LeagueRepository : ILeagueRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public LeagueRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeagueEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.Leagues.FindAsync(id);
    }

    public async Task<LeagueEntity?> GetByNameAsync(string name)
    {
        return await _dbContext.Leagues
            .FirstOrDefaultAsync(l => l.Name == name);
    }

    public async Task<List<LeagueEntity>> GetAllAsync()
    {
        return await _dbContext.Leagues
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<LeagueEntity?> GetFriendsLeagueAsync()
    {
        return await _dbContext.Leagues
            .FirstOrDefaultAsync(l => l.IsFriendsLeague);
    }

    public async Task<List<LeagueEntity>> GetCompetitiveLeaguesAsync()
    {
        return await _dbContext.Leagues
            .Where(l => !l.IsFriendsLeague)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<LeagueEntity?> GetEntryLeagueAsync()
    {
        return await _dbContext.Leagues
            .FirstOrDefaultAsync(l => l.IsEntry);
    }

    public async Task<LeagueEntity> CreateAsync(LeagueEntity league)
    {
        await _dbContext.Leagues.AddAsync(league);
        await _dbContext.SaveChangesAsync();
        return league;
    }

    public async Task<bool> UpdateAsync(LeagueEntity league)
    {
        _dbContext.Leagues.Update(league);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var league = await GetByIdAsync(id);
        if (league == null)
            return false;

        _dbContext.Leagues.Remove(league);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}