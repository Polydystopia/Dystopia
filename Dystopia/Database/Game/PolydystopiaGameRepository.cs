using Dystopia.Bridge;
using Dystopia.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public class PolydystopiaGameRepository : IPolydystopiaGameRepository
{
    private readonly PolydystopiaDbContext _dbContext;
    private readonly ICacheService<GameViewModel> _cacheService;
    private static readonly TimeSpan MaxAccessIntervalForCache = TimeSpan.FromMinutes(5);

    public PolydystopiaGameRepository(PolydystopiaDbContext dbContext, ICacheService<GameViewModel> cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<GameViewModel?> GetByIdAsync(Guid id)
    {
        if (_cacheService.TryGet(id, out GameViewModel? model))
        {
            return model;
        }
        model = await _dbContext.Games.FindAsync(id) ?? null;

        return model;
    }

    private static bool ShouldCache(GameViewModel game)
    {
        if (game.TimerSettings.UseTimebanks)
        {
            return true;
        }

        if (DateTime.Now - game.DateLastCommand < MaxAccessIntervalForCache)
        {
            return true;
        }

        return false;
    }
    public async Task<GameViewModel> CreateAsync(GameViewModel gameViewModel)
    {
        await _dbContext.Games.AddAsync(gameViewModel);
        await _dbContext.SaveChangesAsync();
        return gameViewModel;
    }

    public async Task<GameViewModel> UpdateAsync(GameViewModel gameViewModel)
    {
        if (ShouldCache(gameViewModel))
        {
            _cacheService.Set(gameViewModel.Id, gameViewModel);
            return gameViewModel; // update is automatic as it is a reference type
        }
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
        } // TODO use database for efficient querying

        return playerGames;
    }
}