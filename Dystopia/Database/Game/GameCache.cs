using Dystopia.Services.Cache;

namespace Dystopia.Database.Game;

public static class GameCache
{
    public static ICacheService<GameEntity>? Cache;

    public static void InitializeCache(ICacheService<GameEntity> cache)
    {
        Cache = cache;
    }
}