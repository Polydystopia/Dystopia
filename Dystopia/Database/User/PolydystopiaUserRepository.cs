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
        var model = await _dbContext.Users.FirstOrDefaultAsync(u => u.SteamId == steamId.ToString());

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

    public async Task<bool> UpdateAsync(UserEntity userEntity)
    {
        _dbContext.Users.Update(userEntity);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<UserEntity> CreateAsync(SteamID steamId, string username)
    {
        var steamIdString = steamId.ToString();

        var user = new UserEntity()
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Discriminator = new string(Enumerable.Range(0, 4)
                .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[Random.Shared.Next(36)])
                .ToArray()),
            AllowsFriendRequests = true,
            SteamId = steamIdString,
            Elo = 1000,
            AvatarStateData = SerializationHelpers.ToByteArray(
                AvatarExtensions.CreateRandomState(VersionManager.AvatarDataVersion), VersionManager.GameVersion),
            GameVersions = new List<ClientGameVersionViewModel>()//TODO: Get real game versions
            {
                new()
                {
                    Platform = Platform.Steam,
                    DeviceId = "4c24759ff9d1d0c6e8bb28c7afc178b4752eca0d",
                    GameVersion = 112
                }
            },
            LastLoginDate = DateTime.Now,
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