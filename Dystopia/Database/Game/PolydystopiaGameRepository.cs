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
            model => _bridge.IsPlayerInGame(playerId.ToString(), model.CurrentGameStateData),
            out var cachedPlayerGames);

        var allDbGames = await _dbContext.Games.ToListAsync();
        var dbPlayerGames =
            allDbGames.Where(game =>
                _bridge.IsPlayerInGame(playerId.ToString(), game.CurrentGameStateData) &&
                cachedPlayerGames.All(c => c.Id != game.Id));

        return cachedPlayerGames.Concat(dbPlayerGames).ToList();
    }
}