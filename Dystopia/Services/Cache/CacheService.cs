using System.Collections.Concurrent;
using Dystopia.Database;
using Dystopia.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dystopia.Services.Cache;

public class CacheService<T> : ICacheService<T>
{
    private ConcurrentDictionary<Guid, (T value, DateTime lastUsed, Action<PolydystopiaDbContext> saveToDisk)> _cache = new();
    public CacheService()
    {
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

    public void Set(Guid key, T value, Action<PolydystopiaDbContext> saveToDisk)
    {
        _cache[key] = (value, DateTime.Now, saveToDisk);
    }

    public void TryRemove(Guid key)
    {
        _cache.TryRemove(key, out _);
    }

    public void CleanStaleCache(TimeSpan staleTime, PolydystopiaDbContext dbContext)
    {
        foreach (var (id, (v, lastUsed, saveToDisk)) in _cache)
        {
            if (DateTime.Now - lastUsed > staleTime)
            {
                TryRemove(id);
                saveToDisk(dbContext);
            }
        }
    }
}