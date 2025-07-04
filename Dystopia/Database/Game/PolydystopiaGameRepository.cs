using Dystopia.Bridge;
using Dystopia.Services.Cache;
using Dystopia.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public class PolydystopiaGameRepository : IPolydystopiaGameRepository
{
    private readonly PolydystopiaDbContext _dbContext;
    private readonly ICacheService<GameViewModel> _cacheService;
    private readonly TimeSpan _maxAccessIntervalForCache;

    public PolydystopiaGameRepository(PolydystopiaDbContext dbContext, ICacheService<GameViewModel> cacheService, IOptions<CacheSettings> settings)
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
        var bridge = new DystopiaBridge();

        var allIds = await _dbContext.Games
            .Select(g => g.Id)
            .ToListAsync();

        foreach (var id in allIds)
        {
            GameViewModel? game;

            if (_cacheService.TryGet(id, out var cached))
            {
                game = cached;
            }
            else
            {
                game = await _dbContext.Games.FindAsync(id);

                if (game == null) continue;

                if (ShouldCache(game))
                {
                    _cacheService.Set(game.Id, game, ctx => ctx.Games.Update(game));
                }
            }

            if (bridge.IsPlayerInGame(playerId.ToString(), game.CurrentGameStateData))
            {
                playerGames.Add(game);
            }
        }

        return playerGames;
    }
}