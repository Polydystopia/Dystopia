using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public interface IPolydystopiaGameRepository
{
    public Task<GameEntity?> GetByIdAsync(Guid id);
    public Task<GameEntity> CreateAsync(GameEntity gameViewModel);
    Task<GameEntity> UpdateAsync(GameEntity gameEntity);
    Task<List<GameEntity>> GetAllGamesByPlayer(Guid playerId);
}