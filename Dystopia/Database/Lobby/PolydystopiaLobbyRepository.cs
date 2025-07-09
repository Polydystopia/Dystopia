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

    public async Task<LobbyEntity> CreateAsync(LobbyEntity lobbyEntity)
    {
        await _dbContext.Lobbies.AddAsync(lobbyEntity);
        await _dbContext.SaveChangesAsync();
        return lobbyEntity;
    }

    public async Task<LobbyEntity> UpdateAsync(LobbyEntity lobbyEntity, LobbyUpdatedReason reason)
    {
        // TODO send update to clients
        _dbContext.Lobbies.Update(lobbyEntity);
        await _dbContext.SaveChangesAsync();
        return lobbyEntity;
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
        return _dbContext.Lobbies.Include(lobbyEntity => lobbyEntity.Participators)
            .Where(lobbyGameViewModel => lobbyGameViewModel.Participators
                .Any(participatorViewModel => participatorViewModel.UserId == playerId))
            .ToList();
    }
}