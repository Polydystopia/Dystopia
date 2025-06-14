namespace PolytopiaB2.Carrier.Database.Matchmaking;

public class PolydystopiaMatchmakingRepository : IPolydystopiaMatchmakingRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public PolydystopiaMatchmakingRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}