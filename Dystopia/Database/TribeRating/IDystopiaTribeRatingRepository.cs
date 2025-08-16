namespace Dystopia.Database.TribeRating;

public interface IDystopiaTribeRatingRepository
{
    Task<IList<TribeRatingEntity>?> GetByUserAsync(Guid userId);
    Task<bool> AddOrUpdateAsync(TribeRatingEntity entity);
}