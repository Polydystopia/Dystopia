namespace Dystopia.Settings;

public class CacheSettings
{
    public CacheProfile GameViewModel { get; init; } = new();
}

public class CacheProfile
{
    /// <summary>
    /// After how long to flush cache to db
    /// </summary>
    public TimeSpan StaleTime { get; init; } = TimeSpan.FromMinutes(10); 
    /// <summary>
    /// How often to check all entries in (game)cache against the stale time
    /// </summary>
    public TimeSpan CacheCleanupFrequency { get; init; } = TimeSpan.FromMinutes(2);
    /// <summary>
    /// 2 write operations on a game within this time will trigger a cache
    /// </summary>
    public TimeSpan CacheTime { get; init; } = TimeSpan.FromMinutes(10);
}