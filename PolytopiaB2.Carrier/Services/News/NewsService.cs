using PolytopiaB2.Carrier.Database.News;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Services.News;

public class NewsService : INewsService
{
    private readonly INewsRepository _newsRepository;

    public NewsService(INewsRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public async Task<List<NewsItem>> GetNews()
    {
        var news = await _newsRepository.GetActiveNewsAsync();

        return news.Select(entity => new NewsItem
        {
            Id = entity.Id,
            Body = entity.Body,
            Link = entity.Link,
            Image = entity.Image,
            Date = entity.GetUnixTimestamp()
        }).ToList();
    }

    public async Task<string> GetSystemMessage()
    {
        var message = await _newsRepository.GetSystemMessageAsync();

        if (string.IsNullOrEmpty(message?.Body))
        {
            return string.Empty;
        }

        return message.Body + $"\n{Guid.NewGuid()}";
    }
}