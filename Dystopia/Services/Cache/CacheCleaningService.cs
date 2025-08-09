using Dystopia.Database;
using Dystopia.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PolytopiaBackendBase.Game;

namespace Dystopia.Services.Cache;
/// <summary>
/// cleans the cache. Must be seperate bc background task
/// </summary>
public class CacheCleaningService : BackgroundService
{
    private readonly ICacheService<GameViewModel> _cacheService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly CacheSettings _settings;

    public CacheCleaningService(IOptions<CacheSettings> settings, ICacheService<GameViewModel> cacheService, IServiceScopeFactory scopeFactory, ILogger<CacheCleaningService> logger)
    {
        _cacheService = cacheService;
        _scopeFactory = scopeFactory;
        _logger = logger;
        // TODO make it generic
        // but for now we only have game view model caching so its ok
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_settings.GameEntity.CacheCleanupFrequency, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<PolydystopiaDbContext>();
                _cacheService.CleanStaleCache(_settings.GameEntity.StaleTime, dbContext);
                await dbContext.SaveChangesAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during cache cleanup. Service will continue.");
            }
        }
        
        _logger.LogInformation("CacheCleaningService shutting down; Saving all cache to disk");
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<PolydystopiaDbContext>();
            _cacheService.SaveAllCacheToDisk(dbContext);
            await dbContext.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during final cache save during shutdown.");
        }
    }
}