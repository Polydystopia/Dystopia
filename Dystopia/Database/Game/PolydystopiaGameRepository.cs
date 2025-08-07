using Dystopia.Database.User;
using Dystopia.Services.Cache;
using Dystopia.Settings;
using Microsoft.EntityFrameworkCore;
using DystopiaShared;
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
        _maxAccessIntervalForCache = settings.Value.GameEntity.CacheTime;
        _bridge = bridge;
        _maxAccessIntervalForCache = settings.Value.GameEntity.CacheTime;
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

    public async Task<GameEntity> CreateAsync(GameEntity GameEntity)
    {
        await _dbContext.Games.AddAsync(GameEntity);
        await _dbContext.SaveChangesAsync();
        return GameEntity;
    }

    public async Task<GameEntity> UpdateAsync(GameEntity GameEntity)
    {
        if (ShouldCache(GameEntity))
        {
            _cacheService.Set(GameEntity.Id, GameEntity, context => context.Games.Update(GameEntity));
            return GameEntity; // update is automatic as it is a reference type
            // _dbContext.Games.Update(GameEntity);
            // await _dbContext.SaveChangesAsync();
            // Add this if it is catastrophic when live games or last few moves of games are deleted on server crash.
        }

        _dbContext.Games.Update(GameEntity);
        await _dbContext.SaveChangesAsync();

        return GameEntity;
    }

    public async Task<List<GameEntity>> GetAllGamesByPlayer(Guid playerId)
    {
        var playerIdStr = playerId.ToString();

        _cacheService.TryGetAll(
            game => _bridge.IsPlayerInGame(playerIdStr, game.CurrentGameStateData),
            out var cachedPlayerGames);

        var activeCachedGames = cachedPlayerGames
            .Where(g => g.State != GameSessionState.Ended)
            .ToList();

        var allDbGames = await _dbContext.Games.ToListAsync();
        var dbPlayerGames = allDbGames.Where(game =>
            game.State != GameSessionState.Ended &&
            _bridge.IsPlayerInGame(playerIdStr, game.CurrentGameStateData) &&
            cachedPlayerGames.All(c => c.Id != game.Id));

        return activeCachedGames.Concat(dbPlayerGames).ToList();
    }

    public async Task<List<GameEntity>> GetLastEndedGamesByPlayer(Guid playerId, int limit)
    {
        _cacheService.TryGetAll(
            game =>
                game.State == GameSessionState.Ended &&
                _bridge.IsPlayerInGame(playerId.ToString(), game.CurrentGameStateData),
            out var cachedPlayerGames);

        var allDbGames = await _dbContext.Games.ToListAsync();
        var dbPlayerGames =
            allDbGames.Where(game =>
                game.State == GameSessionState.Ended &&
                _bridge.IsPlayerInGame(playerId.ToString(), game.CurrentGameStateData) &&
                cachedPlayerGames.All(c => c.Id != game.Id));

        return cachedPlayerGames
            .Concat(dbPlayerGames)
            .OrderByDescending(game => game.DateLastCommand)
            .Take(limit)
            .ToList();
    }



    public async Task<List<GameEntity>> GetFavoriteGamesByPlayer(UserEntity user)
    {
        var favGameIds = user.FavoriteGames.Select(g => g.Id).ToList();

        if (!favGameIds.Any())
        {
            return new List<GameEntity>();
        }

        var cachedGames = favGameIds
            .Select(id => _cacheService.TryGet(id, out var g) ? g : null)
            .Where(g => g is { State: GameSessionState.Ended })
            .ToList()!;

        var cachedIds = cachedGames.Select(g => g.Id).ToHashSet();

        var dbGames = await _dbContext.Games
            .Where(g => favGameIds.Contains(g.Id) && !cachedIds.Contains(g.Id) && g.State == GameSessionState.Ended)
            .ToListAsync();

        return cachedGames
            .Concat(dbGames)
            .OrderByDescending(g => g.DateLastCommand)
            .ToList();
    }
}