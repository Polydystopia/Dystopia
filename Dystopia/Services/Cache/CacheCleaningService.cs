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
    private readonly IServiceProvider _provider;
    private GameCacheSettings _settings;
    public CacheCleaningService(IOptions<GameCacheSettings> settings, ICacheService<GameViewModel> cacheService, IServiceProvider provider)
    {
        _cacheService = cacheService;
        _provider = provider;
        // TODO make it generic
        // but for now we only have game view model cahcing so it sok
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(_settings.CacheCleanupFrequency, token);
            using (var scope = _provider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PolydystopiaDbContext>();
                _cacheService.CleanStaleCache(_settings.StaleTime, dbContext);
                await dbContext.SaveChangesAsync(token);
            }
        }
    }
}