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

    public async Task<GameEntity> CreateAsync(GameEntity gameEntity)
    {
        await _dbContext.Games.AddAsync(gameEntity);
        await _dbContext.SaveChangesAsync();
        return gameEntity;
    }

    public async Task<GameEntity> UpdateAsync(GameEntity gameEntity)
    {
        if (ShouldCache(gameEntity))
        {
            _cacheService.Set(gameEntity.Id, gameEntity, context => context.Games.Update(gameEntity));
            return gameEntity;
        }
        _dbContext.Games.Update(gameEntity);
        await _dbContext.SaveChangesAsync();

        return gameEntity;
    }

    public async Task<List<GameEntity>> GetAllGamesByPlayer(Guid playerId)
    {
        // no need to cache as it is usually not relevant. TODO fix when custom entities are implemented(just have a list of playerid in game)
        var allGames = await _dbContext.Games.ToListAsync();
        return allGames
            .Where(game => _bridge.IsPlayerInGame(playerId.ToString(), game.CurrentGameStateData)).ToList();
    }
}