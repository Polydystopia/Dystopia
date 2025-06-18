using Microsoft.EntityFrameworkCore;

namespace PolytopiaB2.Carrier.Database.News;

public class NewsRepository : INewsRepository
{
    private readonly PolydystopiaDbContext _context;

    public NewsRepository(PolydystopiaDbContext context)
    {
        _context = context;
    }

    public async Task<List<NewsEntity>> GetActiveNewsAsync()
    {
        return await _context.News
            .Where(n => n.IsActive && n.NewsType == NewsType.News)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<NewsEntity?> GetSystemMessageAsync()
    {
        return await _context.News
            .Where(n => n.IsActive && n.NewsType == NewsType.SystemMessage)
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<NewsEntity> CreateAsync(NewsEntity news)
    {
        news.CreatedAt = DateTime.UtcNow;
        _context.News.Add(news);
        await _context.SaveChangesAsync();
        return news;

    }

    public async Task<NewsEntity> UpdateAsync(NewsEntity news)
    {
        news.UpdatedAt = DateTime.UtcNow;
        _context.News.Update(news);
        await _context.SaveChangesAsync();
        return news;

    }

    public async Task DeleteAsync(int id)
    {
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            news.IsActive = false;
            news.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}