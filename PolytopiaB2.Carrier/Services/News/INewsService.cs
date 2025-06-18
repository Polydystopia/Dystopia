using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Services.News;

public interface INewsService
{
    List<NewsItem> GetNews();
}