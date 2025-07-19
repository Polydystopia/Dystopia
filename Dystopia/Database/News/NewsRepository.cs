using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace Dystopia.Database.News;

public class NewsRepository : INewsRepository
{
    private readonly PolydystopiaDbContext _dbContext;
    public NewsRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<NewsEntity>> GetActiveNewsAsync()
    {
        return await _dbContext.News
            .Where(n => n.IsActive && n.NewsType == NewsType.News)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<string?> GetSystemMessageAsync()
    {
        return await _dbContext.News
            .Where(n => n.IsActive && n.NewsType == NewsType.SystemMessage)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => n.Body)
            .FirstOrDefaultAsync();
    }

    public async Task<NewsEntity> CreateAsync(NewsEntity news)
    {
        news.CreatedAt = DateTime.UtcNow;
        _dbContext.News.Add(news);
        await _dbContext.SaveChangesAsync();
        
        
        return news;
    }

    public async Task<NewsEntity> UpdateAsync(NewsEntity news)
    {
        news.UpdatedAt = DateTime.UtcNow;
        _dbContext.News.Update(news);
        await _dbContext.SaveChangesAsync();
        return news;

    }

    public async Task DeleteAsync(int id)
    {
        var news = await _dbContext.News.FindAsync(id);
        if (news != null)
        {
            news.IsActive = false;
            news.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}