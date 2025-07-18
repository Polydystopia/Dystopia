using PolytopiaBackendBase.Game;

namespace Dystopia.Services.News;

public interface INewsService
{
    Task<IReadOnlyCollection<NewsItem>> GetNews();

    Task<string?> GetSystemMessage();
}