using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Lobby;

public interface IPolydystopiaLobbyRepository
{
    public Task<LobbyEntity?> GetByIdAsync(Guid id);
    public Task<LobbyEntity> CreateAsync(LobbyEntity lobbyGameViewModel);
    Task<LobbyEntity> UpdateAsync(LobbyEntity lobbyGameViewModel, LobbyUpdatedReason reason);
    Task<bool> DeleteAsync(Guid id);
    Task<List<LobbyEntity>> GetAllLobbiesByPlayer(Guid playerId);
}