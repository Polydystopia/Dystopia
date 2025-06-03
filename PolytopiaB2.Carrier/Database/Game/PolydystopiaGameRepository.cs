using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Game;

public class PolydystopiaGameRepository : IPolydystopiaGameRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public PolydystopiaGameRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<GameViewModel?> GetByIdAsync(Guid id)
    {
        var model = await _dbContext.Games.FindAsync(id) ?? null;

        return model;
    }
    
    public async Task<GameViewModel> CreateAsync(GameViewModel gameViewModel)
    {
        await _dbContext.Games.AddAsync(gameViewModel);
        await _dbContext.SaveChangesAsync();
        return gameViewModel;
    }
}