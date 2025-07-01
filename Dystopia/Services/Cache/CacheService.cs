using System.Collections.Concurrent;

namespace Dystopia.Services.Cache;

public class CacheService<T> : ICacheService<T>
{
    private ConcurrentDictionary<Guid, (T value, DateTime lastUsed)> _cache = new();

    public CacheService()
    {
        CleanCacheInBackground( /* HOW TF DO I SHARE CONSTANTS DO I NEED TO MAKE ENTIRE NEW CLASS FOR THIS */)
    }
    public bool TryGet(Guid key, out T? value)
    {
        if (_cache.TryGetValue(key, out var valueTuple))
        {
            value = valueTuple.value;
            return true;
        }
        value = default;
        return false;
    }

    public void Set(Guid key, T value)
    {
        _cache[key] = (value, DateTime.Now);
    }

    public void TryRemove(Guid key)
    {
        _cache.TryRemove(key, out _);
    }

    private void CleanStaleCache(TimeSpan staleTime)
    {
        foreach (var (id, (_, lastUsed)) in _cache)
        {
            if (DateTime.Now - lastUsed > staleTime)
            {
                TryRemove(id);
            }
        }
    }

    private async Task CleanCacheInBackground(TimeSpan staleTime, TimeSpan delay)
    {
        while (true)
        {
            await Task.Delay(delay);
            CleanStaleCache(staleTime);
        }
    }
}