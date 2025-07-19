using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Matchmaking;

public class PolydystopiaMatchmakingRepository : IPolydystopiaMatchmakingRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public PolydystopiaMatchmakingRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MatchmakingEntity> CreateAsync(MatchmakingEntity matchmakingEntity)
    {
        await _dbContext.Matchmaking.AddAsync(matchmakingEntity);
        await _dbContext.SaveChangesAsync();
        return matchmakingEntity;
    }

    public async Task<MatchmakingEntity> UpdateAsync(MatchmakingEntity matchmakingEntity)
    {
        _dbContext.Matchmaking.Update(matchmakingEntity);
        await _dbContext.SaveChangesAsync();
        return matchmakingEntity;
    }

    private IQueryable<MatchmakingEntity> GetAllFittingLobbiesCommon(MatchMakingFilter filter)
    {
        var query = _dbContext.Matchmaking
            .Include(m => m.LobbyGameViewModel)
            .Where(m =>
                m.Version == filter.Version &&
                m.TimeLimit == filter.TimeLimit &&
                (m.Platform == filter.Platform || 
                 (m.AllowCrossPlay && filter.AllowCrossPlay)) &&
                m.Players.Count < m.MaxPlayers &&
                m.Players.All(u => u.PolytopiaId != filter.PlayerId));

        if (filter.MapSize != 0)
        {
            query = query.Where(m => m.MapSize == filter.MapSize);
        }

        if (filter.MapPreset != MapPreset.None)
        {
            query = query.Where(m => m.MapPreset == filter.MapPreset);
        }

        if (filter.GameMode != GameMode.None)
        {
            query = query.Where(m => m.GameMode == filter.GameMode);
        }

        if (filter.ScoreLimit != 0)
        {
            query = query.Where(m => m.ScoreLimit == filter.ScoreLimit);
        }

        return query;
    }

    public Task<List<MatchmakingEntity>> GetAllFittingLobbies(MatchMakingFilter matchMakingFilter)
    {
        return GetAllFittingLobbiesCommon(matchMakingFilter).ToListAsync();
    }
    
    public Task<List<MatchmakingEntity>> GetAllFittingLobbiesOrderedByAmountOfPlayers(
        MatchMakingFilter matchMakingFilter)
    {
        return GetAllFittingLobbiesCommon(matchMakingFilter)
            .OrderByDescending(m => m.Players.Count)
            .ToListAsync();
    }
    public Task<MatchmakingEntity?> GetMostFittingLobbyOrDefault(MatchMakingFilter matchMakingFilter)
    {
        return GetAllFittingLobbiesCommon(matchMakingFilter)
            .OrderByDescending(m => m.Players.Count)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteByIdAsync(Guid lobbyId)
    {
        var matchmaking = await _dbContext.Matchmaking.FindAsync(lobbyId);
        if (matchmaking == null) return false;

        _dbContext.Matchmaking.Remove(matchmaking);
        await _dbContext.SaveChangesAsync();

        return true;

    }
}