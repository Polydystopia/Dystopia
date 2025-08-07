using Dystopia.Database.User;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public interface IPolydystopiaGameRepository
{
    public Task<GameEntity?> GetByIdAsync(Guid id);
    public Task<GameEntity> CreateAsync(GameEntity gameEntity);
    Task<GameEntity> UpdateAsync(GameEntity gameEntity);

    Task<List<GameEntity>> GetAllGamesByPlayer(Guid playerId);
    Task<List<GameEntity>> GetLastEndedGamesByPlayer(Guid playerId, int limit);

    Task<List<GameEntity>> GetFavoriteGamesByPlayer(UserEntity user);
}