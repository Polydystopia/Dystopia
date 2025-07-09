using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Lobby;

public class PolydystopiaLobbyRepository : IPolydystopiaLobbyRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public PolydystopiaLobbyRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LobbyEntity?> GetByIdAsync(Guid id)
    {
        var model = await _dbContext.Lobbies.FindAsync(id) ?? null;

        return model;
    }

    public async Task<LobbyEntity> CreateAsync(LobbyEntity lobbyGameViewModel)
    {
        await _dbContext.Lobbies.AddAsync(lobbyGameViewModel);
        await _dbContext.SaveChangesAsync();
        return lobbyGameViewModel;
    }

    public async Task<LobbyEntity> UpdateAsync(LobbyEntity lobbyGameViewModel, LobbyUpdatedReason reason)
    {
        // TODO send update to clients
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

    public async Task<List<LobbyEntity>> GetAllLobbiesByPlayer(Guid playerId)
    {
        var playerLobbies = new List<LobbyEntity>();
        
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