using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.News;

public interface INewsRepository
{
    Task<List<NewsEntity>> GetActiveNewsAsync();
    Task<NewsEntity?> GetSystemMessageAsync();
    Task<NewsEntity> CreateAsync(NewsEntity news);
    Task<NewsEntity> UpdateAsync(NewsEntity news);
    Task DeleteAsync(int id);
}