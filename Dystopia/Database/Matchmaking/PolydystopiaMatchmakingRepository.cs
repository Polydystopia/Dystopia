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

    public async Task<List<MatchmakingEntity>> GetAllFittingLobbies(Guid playerId, int version, int mapSize, MapPreset mapPreset, GameMode gameMode, int scoreLimit,
        int timeLimit, Platform platform, bool allowCrossPlay)
    {
        var query = _dbContext.Matchmaking
            .Include(m => m.LobbyEntity)
            .Where(m =>
                m.Version   == version &&
                m.TimeLimit == timeLimit &&
                (m.Platform == platform || (m.AllowCrossPlay && allowCrossPlay))
            );

        if (mapSize != 0)
        {
            query = query.Where(m => m.MapSize == mapSize);
        }

        if (mapPreset != MapPreset.None)
        {
            query = query.Where(m => m.MapPreset == mapPreset);
        }

        if (gameMode != GameMode.None)
        {
            query = query.Where(m => m.GameMode == gameMode);
        }

        if (scoreLimit != 0)
        {
            query = query.Where(m => m.ScoreLimit == scoreLimit);
        }

        var matchingLobbies = await query.ToListAsync();

        return matchingLobbies
            .Where(m => m.PlayerIds.Count < m.MaxPlayers &&
                        !m.PlayerIds.Contains(playerId))
            .ToList();
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