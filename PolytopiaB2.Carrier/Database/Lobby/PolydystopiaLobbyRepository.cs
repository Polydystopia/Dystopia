using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Lobby;

public class PolydystopiaLobbyRepository : IPolydystopiaLobbyRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public PolydystopiaLobbyRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LobbyGameViewModel?> GetByIdAsync(Guid id)
    {
        var model = await _dbContext.Lobbies.FindAsync(id) ?? null;

        return model;
    }

    public async Task<LobbyGameViewModel> CreateAsync(LobbyGameViewModel lobbyGameViewModel)
    {
        await _dbContext.Lobbies.AddAsync(lobbyGameViewModel);
        await _dbContext.SaveChangesAsync();
        return lobbyGameViewModel;
    }

    public async Task<LobbyGameViewModel> UpdateAsync(LobbyGameViewModel lobbyGameViewModel, LobbyUpdatedReason reason)
    {
        lobbyGameViewModel.DateModified = DateTime.UtcNow;
        lobbyGameViewModel.UpdatedReason = reason;
        
        _dbContext.Lobbies.Update(lobbyGameViewModel);
        await _dbContext.SaveChangesAsync();
        return lobbyGameViewModel;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var lobby = await _dbContext.Lobbies.FindAsync(id);
    
        if (lobby == null)
        {
            return false;
        }
    
        _dbContext.Lobbies.Remove(lobby);
        await _dbContext.SaveChangesAsync();
    
        return true;
    }

    public async Task<List<LobbyGameViewModel>> GetAllLobbiesByPlayer(Guid playerId)
    {
        var playerLobbies = new List<LobbyGameViewModel>();
        
        foreach (var lobbyGameViewModel in _dbContext.Lobbies)
        {
            foreach (var participatorViewModel in lobbyGameViewModel.Participators)
            {
                if (participatorViewModel.UserId == playerId)
                {
                    playerLobbies.Add(lobbyGameViewModel);
                    break;
                }
            }
        }

        return playerLobbies;
    }
}