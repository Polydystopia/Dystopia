using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Game;

public interface IPolydystopiaGameRepository
{
    public Task<GameViewModel?> GetByIdAsync(Guid id);
    public Task<GameViewModel> CreateAsync(GameViewModel gameViewModel);
    Task<GameViewModel> UpdateAsync(GameViewModel gameViewModel);

    Task<List<GameViewModel>> GetAllGamesByPlayer(Guid playerId);
    Task<List<GameViewModel>> GetLastEndedGamesByPlayer(Guid playerId, int limit);
}