using System.Collections.Concurrent;
using Dystopia.Database;
using Dystopia.Database.News;
using PolytopiaBackendBase.Game;

namespace Dystopia.Services.News;

public class NewsService : INewsService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private ConcurrentBag<NewsItem> _news = new();
    private string? _systemMessage = "";
    private TaskCompletionSource _isInitialized = new();
    public NewsService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Initialize()
    {
        using var scope = _scopeFactory.CreateScope();
        var newsRepository = scope.ServiceProvider.GetRequiredService<INewsRepository>();
        
        var newsEntities = await newsRepository.GetActiveNewsAsync();
        var newsItems = newsEntities.Select(n => (NewsItem)n);
        // var newsItems = newsRepository.GetActiveNewsAsync()
        //     .Then(n => n.Select(n => (NewsItem)n));
        _news = new ConcurrentBag<NewsItem>(newsItems); 
        _systemMessage = await newsRepository.GetSystemMessageAsync();
        
        _isInitialized.SetResult();
    }
    public async Task<IReadOnlyCollection<NewsItem>> GetNews()
    {
        await _isInitialized.Task;
        return _news;   
    }

    public async Task<string?> GetSystemMessage()
    {
        await _isInitialized.Task;
        return _systemMessage;
    }
}