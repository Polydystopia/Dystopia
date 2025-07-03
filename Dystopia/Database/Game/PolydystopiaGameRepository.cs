using Dystopia.Bridge;
using Dystopia.Services.Cache;
using Dystopia.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public class PolydystopiaGameRepository : IPolydystopiaGameRepository
{
    private readonly PolydystopiaDbContext _dbContext;
    private readonly ICacheService<GameViewModel> _cacheService;
    private readonly TimeSpan _maxAccessIntervalForCache;

    public PolydystopiaGameRepository(PolydystopiaDbContext dbContext, ICacheService<GameViewModel> cacheService, IOptions<GameCacheSettings> settings)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _maxAccessIntervalForCache = settings.Value.GameViewModel.CacheTime;
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

    private bool ShouldCache(GameViewModel game)
    {
        if (game.TimerSettings.UseTimebanks)
        {
            return true;
        }

        if (DateTime.Now - game.DateLastCommand < _maxAccessIntervalForCache)
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
            _cacheService.Set(gameViewModel.Id, gameViewModel, context => context.Games.Update(gameViewModel));
            return gameViewModel; // update is automatic as it is a reference type
            // _dbContext.Games.Update(gameViewModel);
            // await _dbContext.SaveChangesAsync();
            // Add this if it is catastrophic when live games or last few moves of games are deleted on server crash.
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