using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Matchmaking;

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
        var matchingLobbies = await _dbContext.Matchmaking
            .Include(m => m.LobbyGameViewModel)
            .Where(m => m.Version == version &&
                        m.MapSize == mapSize &&
                        m.MapPreset == mapPreset &&
                        m.GameMode == gameMode &&
                        m.ScoreLimit == scoreLimit &&
                        m.TimeLimit == timeLimit &&
                        (m.Platform == platform || (m.AllowCrossPlay && allowCrossPlay)))
            .ToListAsync();

        return matchingLobbies
            .Where(m => m.PlayerIds.Count < m.MaxPlayers &&
                        !m.PlayerIds.Contains(playerId))
            .ToList();
    }
}