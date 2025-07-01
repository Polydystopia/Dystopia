namespace Dystopia.Services.Cache
{
    public interface ICacheService<T>
    {
        bool TryGet(Guid ke, out T? value);
        void Set(Guid key, T value);
        void TryRemove(Guid key);
    }
}