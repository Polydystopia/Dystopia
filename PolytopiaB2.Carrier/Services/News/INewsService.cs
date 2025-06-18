using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Services.News;

public interface INewsService
{
    Task<List<NewsItem>> GetNews();

    Task<string> GetSystemMessage();
}