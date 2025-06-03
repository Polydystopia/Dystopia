using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Lobby;

public interface IPolydystopiaLobbyRepository
{
    public Task<LobbyGameViewModel?> GetByIdAsync(Guid id);
    public Task<LobbyGameViewModel> CreateAsync(LobbyGameViewModel lobbyGameViewModel);
    Task<LobbyGameViewModel> UpdateAsync(LobbyGameViewModel lobbyGameViewModel, LobbyUpdatedReason reason);
    
    Task<bool> DeleteAsync(Guid id);
}