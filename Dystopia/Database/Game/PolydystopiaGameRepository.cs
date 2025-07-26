using Dystopia.Bridge;
using Dystopia.Database.Replay;
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
    private readonly ICacheService<GameViewModel> _cacheService;
    private readonly IDystopiaCastle _bridge;
    private readonly TimeSpan _maxAccessIntervalForCache;

    public PolydystopiaGameRepository(PolydystopiaDbContext dbContext,
        ICacheService<GameViewModel> cacheService,
        IOptions<CacheSettings> settings,
        IDystopiaCastle bridge)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _maxAccessIntervalForCache = settings.Value.GameViewModel.CacheTime;
        _bridge = bridge;
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
        _cacheService.TryGetAll(
            game =>
                game.State != GameSessionState.Ended &&
                _bridge.IsPlayerInGame(playerId.ToString(), game.CurrentGameStateData),
            out var cachedPlayerGames);

        var allDbGames = await _dbContext.Games.ToListAsync();
        var dbPlayerGames =
            allDbGames.Where(game =>
                game.State != GameSessionState.Ended &&
                _bridge.IsPlayerInGame(playerId.ToString(), game.CurrentGameStateData) &&
                cachedPlayerGames.All(c => c.Id != game.Id));

        return cachedPlayerGames.Concat(dbPlayerGames).ToList();
    }

    public async Task<List<GameViewModel>> GetLastEndedGamesByPlayer(Guid playerId, int limit)
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

    public async Task<List<GameViewModel>> GetFavoriteGamesByPlayer(Guid playerId)
    {
        var favGameIds = await _dbContext.UserFavoriteGames
            .Where(uf => uf.UserId == playerId)
            .Select(uf => uf.GameId)
            .ToListAsync();

        if (!favGameIds.Any())
        {
            return new List<GameViewModel>();
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


    public async Task AddFavoriteAsync(Guid userId, Guid gameId)
    {
        var fav = new UserFavoriteGame
        {
            UserId = userId,
            GameId = gameId
        };

        _dbContext.UserFavoriteGames.Add(fav);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveFavoriteAsync(Guid userId, Guid gameId)
    {
        var favorite = await _dbContext.UserFavoriteGames
            .FirstOrDefaultAsync(f => f.UserId == userId && f.GameId == gameId);

        if (favorite != null)
        {
            _dbContext.UserFavoriteGames.Remove(favorite);
            await _dbContext.SaveChangesAsync();
        }
    }
}