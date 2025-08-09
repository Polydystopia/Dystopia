using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Lobby;

public interface IPolydystopiaLobbyRepository
{
    public Task<LobbyEntity?> GetByIdAsync(Guid id);
    public Task<LobbyEntity> CreateAsync(LobbyEntity lobbyEntity);
    Task<LobbyEntity> UpdateAsync(LobbyEntity lobbyEntity);
    Task<bool> DeleteAsync(Guid id);
    Task<List<LobbyEntity>> GetAllLobbiesByPlayer(Guid playerId);
}