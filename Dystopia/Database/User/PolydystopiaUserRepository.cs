using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Common;
using SteamKit2;

namespace Dystopia.Database.User;

//TODO: UserName can not be set. We use SteamId instead. But we need to call AddMissingData() to set the correct values. This is a hack.
public class PolydystopiaUserRepository : IPolydystopiaUserRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public PolydystopiaUserRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserEntity> GetBySteamIdAsync(SteamID steamId, string username)
    {
        // TODO
        var model = await _dbContext.Users.FirstOrDefaultAsync(u => u.Alias == steamId.ToString());

        if (model == null)
        {
            model = await CreateAsync(steamId, username);
        }

        return model;
    }

    public async Task<UserEntity?> GetByIdAsync(Guid polytopiaId)
    {
        var model = await _dbContext.Users.FindAsync(polytopiaId) ?? null;

        return model;
    }

    public async Task<bool> UpdateAsync(UserEntity userViewModel)
    {
        _dbContext.Users.Update(userViewModel);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task<string> FindDiscriminator(string alias)
    {
        var discriminators = _dbContext.Users
            .Where(user => user.Alias == alias)
            .Select(user => user.Discriminator);
        while (true)
        {
            var intendedDiscriminator = Random.Shared.Next(9999) // 0000 to 9999
                .ToString(); 
            if (!await discriminators.ContainsAsync(intendedDiscriminator))
            {
                return intendedDiscriminator;
            }
        }
    }
    public async Task<UserEntity> CreateAsync(SteamID steamId, string username)
    {

        var user = new UserEntity
        {
            PolytopiaId = Guid.NewGuid(),
            Alias = username,
            Discriminator = await FindDiscriminator(username),
            SteamId = steamId,
            AllowsFriendRequests = true,
            Elo = 1000,
            Games = null,
            MatchmakingEntities = null,
            AvatarStateData = new byte[]
            {
            },
            LastLoginDate = DateTime.UtcNow,
            Friends1 = null,
            Friends2 = null
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<List<UserEntity>> GetAllByNameStartsWith(string name)
    {
        var foundUsers = await _dbContext.Users
            .WhereUserNameStartsWith(name)
            .ToListAsync();

        return foundUsers;
    }
}

public static class QueryExtensions
{
    public static IQueryable<UserEntity> WhereUserNameStartsWith(
        this IQueryable<UserEntity> query, string name)
    {
        return query.Where(user => EF.Property<string>(user, "UserName").StartsWith(name));
    }

    public static IQueryable<UserEntity> WhereAliasStartsWith(
        this IQueryable<UserEntity> query, string name)
    {
        return query.Where(user => EF.Property<string>(user, "Alias").StartsWith(name));
    }
}