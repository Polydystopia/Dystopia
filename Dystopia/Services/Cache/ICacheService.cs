using Dystopia.Database;
using Microsoft.EntityFrameworkCore;

namespace Dystopia.Services.Cache
{
    public interface ICacheService<T>
    {
        bool TryGet(Guid ke, out T? value);
        void Set(Guid key, T value, Action<PolydystopiaDbContext> saveToDisk);
        void TryRemove(Guid key);
        void CleanStaleCache(TimeSpan staleTime, PolydystopiaDbContext dbContext);
    }
}