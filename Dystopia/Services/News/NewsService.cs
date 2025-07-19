using System.Collections.Concurrent;
using System.Collections.Immutable;
using Dystopia.Database;
using Dystopia.Database.News;
using PolytopiaBackendBase.Game;

namespace Dystopia.Services.News;

public class NewsService : INewsService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private ImmutableList<NewsItem> _news = ImmutableList<NewsItem>.Empty;
    private string _systemMessage;
    private bool _isInitialized;
    public NewsService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private async Task Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        using var scope = _scopeFactory.CreateScope();
        var newsRepository = scope.ServiceProvider.GetRequiredService<INewsRepository>();
        
        var newsEntities = await newsRepository.GetActiveNewsAsync();
        var newsItems = newsEntities.Select(n => (NewsItem)n);
        // var newsItems = newsRepository.GetActiveNewsAsync()
        //     .Then(n => n.Select(n => (NewsItem)n));
        _news = newsItems.ToImmutableList(); 
        _systemMessage = await newsRepository.GetSystemMessageAsync() ?? string.Empty;

        _isInitialized = true;
    }
    public async Task<IReadOnlyCollection<NewsItem>> GetNews()
    {
        await Initialize();
        return _news;   
    }

    public async Task<string> GetSystemMessage()
    {
        await Initialize();
        return _systemMessage;
    }

    public async Task Reinitialize()
    {
        _isInitialized = false;
        await Initialize();
    }
}