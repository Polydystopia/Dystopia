using PolytopiaBackendBase.Game;

namespace Dystopia.Services.News;

public interface INewsService
{
    Task<List<NewsItem>> GetNews();

    Task<string> GetSystemMessage();
}