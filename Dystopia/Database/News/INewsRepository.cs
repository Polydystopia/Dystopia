using PolytopiaBackendBase.Game;

namespace Dystopia.Database.News;

public interface INewsRepository
{
    Task<List<NewsEntity>> GetActiveNewsAsync();
    Task<string?> GetSystemMessageAsync();
    Task<NewsEntity> CreateAsync(NewsEntity news);
    Task<NewsEntity> UpdateAsync(NewsEntity news);
    Task DeleteAsync(int id);
}