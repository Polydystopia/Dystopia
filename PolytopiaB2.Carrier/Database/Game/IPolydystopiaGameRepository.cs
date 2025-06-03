using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Game;

public interface IPolydystopiaGameRepository
{
    public Task<GameViewModel?> GetByIdAsync(Guid id);
    public Task<GameViewModel> CreateAsync(GameViewModel gameViewModel);
}