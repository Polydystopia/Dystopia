using Microsoft.EntityFrameworkCore;

namespace Dystopia.Database.TribeRating;

public class DystopiaTribeRatingRepository : IDystopiaTribeRatingRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public DystopiaTribeRatingRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IList<TribeRatingEntity>?> GetByUserAsync(Guid userId)
    {
        return await _dbContext.TribeRatings
            .Include(tr => tr.User)
            .Where(tr => tr.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> AddOrUpdateAsync(TribeRatingEntity entity)
    {
        var existing = await _dbContext.TribeRatings
            .FirstOrDefaultAsync(tr => tr.UserId == entity.UserId && tr.Tribe == entity.Tribe);

        if (existing != null)
        {
            bool updated = false;
            
            if (entity.Score.HasValue && (!existing.Score.HasValue || entity.Score > existing.Score))
            {
                existing.Score = entity.Score;
                updated = true;
            }
            
            if (entity.Rating.HasValue && (!existing.Rating.HasValue || entity.Rating > existing.Rating))
            {
                existing.Rating = entity.Rating;
                updated = true;
            }
            
            if (updated)
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            
            return false;
        }
        else
        {
            _dbContext.TribeRatings.Add(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}