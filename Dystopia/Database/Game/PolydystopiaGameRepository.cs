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

    public async Task<List<GameEntity>> GetAllGamesByPlayer(UserEntity user)
    {
        var activeParticipations = user.GameParticipations
            .Where(g => g.ActualGame.State != GameSessionState.Ended).Select(g => g.ActualGame)
            .ToList();

        return activeParticipations;
    }

    public async Task<List<GameEntity>> GetLastEndedGamesByPlayer(UserEntity user, int limit)
    {
        var endedParticipations = user.GameParticipations
            .Where(g => g.ActualGame.State == GameSessionState.Ended).Select(g => g.ActualGame)
            .OrderByDescending(game => game.DateLastCommand)
            .Take(limit);

        return endedParticipations.ToList();
    }

    public async Task<List<GameEntity>> GetFavoriteGamesByPlayer(UserEntity user)
    {
        return user.ActualFavoriteGames.OrderByDescending(g => g.DateLastCommand).ToList();
    }
}