using Dystopia.Database;
using Microsoft.EntityFrameworkCore;

namespace Dystopia.Services.Cache
{
    public interface ICacheService<T>
    {
        bool TryGet(Guid key, out T? value);
        void TryGetAll(Func<T, bool> predicate, out IList<T> values);
        void Set(Guid key, T value, Action<PolydystopiaDbContext> saveToDisk);
        void TryRemove(Guid key);
        void CleanStaleCache(TimeSpan staleTime, PolydystopiaDbContext dbContext);
        void SaveAllCacheToDisk(PolydystopiaDbContext dbContext);
    }
}