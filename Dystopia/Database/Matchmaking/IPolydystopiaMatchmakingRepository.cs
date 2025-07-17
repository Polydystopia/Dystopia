using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Matchmaking;

public interface IPolydystopiaMatchmakingRepository
{
    Task<MatchmakingEntity> CreateAsync(MatchmakingEntity matchmakingEntity);
    Task<MatchmakingEntity> UpdateAsync(MatchmakingEntity matchmakingEntity);

    Task<List<MatchmakingEntity>> GetAllFittingLobbies(MatchMakingFilter matchMakingFilter);

    Task<List<MatchmakingEntity>> GetAllFittingLobbiesOrderedByAmountOfPlayers(MatchMakingFilter matchMakingFilter);

    Task<MatchmakingEntity?> GetMostFittingLobbyOrDefault(MatchMakingFilter matchMakingFilter);

    Task<bool> DeleteByIdAsync(Guid lobbyId);
}