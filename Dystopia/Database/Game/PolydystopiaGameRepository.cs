using Dystopia.Bridge;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public class PolydystopiaGameRepository : IPolydystopiaGameRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public PolydystopiaGameRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GameViewModel?> GetByIdAsync(Guid id)
    {
        var model = await _dbContext.Games.FindAsync(id) ?? null;

        return model;
    }

    public async Task<GameViewModel> CreateAsync(GameViewModel gameViewModel)
    {
        await _dbContext.Games.AddAsync(gameViewModel);
        await _dbContext.SaveChangesAsync();
        return gameViewModel;
    }

    public async Task<GameViewModel> UpdateAsync(GameViewModel gameViewModel)
    {
        _dbContext.Games.Update(gameViewModel);
        await _dbContext.SaveChangesAsync();

        return gameViewModel;
    }

    public async Task<List<GameViewModel>> GetAllGamesByPlayer(Guid playerId)
    {
        var playerGames = new List<GameViewModel>();

        foreach (var gameViewModel in _dbContext.Games)
        {
            var bridge = new DystopiaBridge();
            if (bridge.IsPlayerInGame(playerId.ToString(), gameViewModel.CurrentGameStateData))
            {
                playerGames.Add(gameViewModel);
            }
        }

        return playerGames;
    }
}