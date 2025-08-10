using Dystopia.Database.User;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public interface IPolydystopiaGameRepository
{
    public Task<GameEntity?> GetByIdAsync(Guid id);
    public Task<GameEntity> CreateAsync(GameEntity gameEntity);
    Task<GameEntity> UpdateAsync(GameEntity gameEntity);

    Task<List<GameEntity>> GetAllGamesByPlayer(UserEntity user);
    Task<List<GameEntity>> GetLastEndedGamesByPlayer(UserEntity user, int limit);

    Task<List<GameEntity>> GetFavoriteGamesByPlayer(UserEntity user);
}