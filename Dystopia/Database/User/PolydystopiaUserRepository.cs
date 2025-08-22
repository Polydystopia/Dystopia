using Dystopia.Database.Game;
using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using SteamKit2;

namespace Dystopia.Database.User;

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
            GameVersions = new List<ClientGameVersionViewModel>(),
            LastLoginDate = DateTime.Now,
            CurrentLeagueId = 1 //TODO get entry league of db
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

    public async Task AddFavoriteAsync(UserEntity user, GameEntity game)
    {
        if (!_dbContext.Entry(user).Collection(u => u.FavoriteGames).IsLoaded)
        {
            await _dbContext.Entry(user)
                .Collection(u => u.FavoriteGames)
                .LoadAsync();
        }

        if (user.FavoriteGames.All(g => g.Id != game.Id))
        {
            user.FavoriteGames.Add(game);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RemoveFavoriteAsync(UserEntity user, GameEntity game)
    {
        if (!_dbContext.Entry(user).Collection(u => u.FavoriteGames).IsLoaded)
        {
            await _dbContext.Entry(user)
                .Collection(u => u.FavoriteGames)
                .LoadAsync();
        }

        var existing = user.FavoriteGames.FirstOrDefault(g => g.Id == game.Id);
        if (existing != null)
        {
            user.FavoriteGames.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }
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