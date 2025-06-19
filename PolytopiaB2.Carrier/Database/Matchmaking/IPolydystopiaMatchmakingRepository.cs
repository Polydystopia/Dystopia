using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Matchmaking;

public interface IPolydystopiaMatchmakingRepository
{
    Task<MatchmakingEntity> CreateAsync(MatchmakingEntity matchmakingEntity);
    Task<MatchmakingEntity> UpdateAsync(MatchmakingEntity matchmakingEntity);
    Task<List<MatchmakingEntity>> GetAllFittingLobbies(Guid playerId, int version, int mapSize, MapPreset mapPreset, GameMode gameMode, int scoreLimit, int timeLimit, Platform platform, bool allowCrossPlay);
    Task<bool> DeleteByIdAsync(Guid lobbyId);
}