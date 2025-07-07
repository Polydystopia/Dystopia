using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Lobby;

public interface IPolydystopiaLobbyRepository
{
    public Task<LobbyGameViewModel?> GetByIdAsync(Guid id);
    public Task<LobbyGameViewModel> CreateAsync(LobbyGameViewModel lobbyGameViewModel);
    Task<LobbyGameViewModel> UpdateAsync(LobbyGameViewModel lobbyGameViewModel, LobbyUpdatedReason reason);
    Task<bool> DeleteAsync(Guid id);
    Task<List<LobbyGameViewModel>> GetAllLobbiesByPlayer(Guid playerId);
}