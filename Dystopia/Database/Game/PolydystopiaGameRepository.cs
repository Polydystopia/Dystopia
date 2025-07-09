using Dystopia.Bridge;
using Dystopia.Patches;
using Dystopia.Services.Cache;
using Dystopia.Settings;
using Microsoft.EntityFrameworkCore;
using DystopiaShared;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public class PolydystopiaGameRepository : IPolydystopiaGameRepository
{
    private readonly PolydystopiaDbContext _dbContext;
    private readonly ICacheService<GameEntity> _cacheService;
    private readonly IDystopiaCastle _bridge;
    private readonly TimeSpan _maxAccessIntervalForCache;

    public PolydystopiaGameRepository(PolydystopiaDbContext dbContext,
        ICacheService<GameEntity> cacheService,
        IOptions<CacheSettings> settings,
        IDystopiaCastle bridge)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _maxAccessIntervalForCache = settings.Value.GameViewModel.CacheTime;
        _bridge = bridge;
        _maxAccessIntervalForCache = settings.Value.GameViewModel.CacheTime;
    }

    public async Task<GameEntity?> GetByIdAsync(Guid id)
    {
        if (_cacheService.TryGet(id, out GameEntity? model))
        {
            return model;
        }
        model = await _dbContext.Games.FindAsync(id) ?? null;

        return model;
    }

    private bool ShouldCache(GameEntity game)
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

    public async Task<GameEntity> CreateAsync(GameEntity gameViewModel)
    {
        await _dbContext.Games.AddAsync(gameViewModel);
        await _dbContext.SaveChangesAsync();
        return gameViewModel;
    }

    public async Task<GameEntity> UpdateAsync(GameEntity gameViewModel)
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

    public async Task<List<GameEntity>> GetAllGamesByPlayer(Guid playerId)
    {
        // no need to cache as it is usually not relevant. TODO fix when custom entities are implemented(just have a list of playerid in game)
        var allGames = await _dbContext.Games.ToListAsync();
        return allGames
            .Where(game => _bridge.IsPlayerInGame(playerId.ToString(), game.CurrentGameStateData)).ToList();
    }
}