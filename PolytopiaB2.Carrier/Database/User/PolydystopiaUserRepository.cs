using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Common;
using SteamKit2;

namespace PolytopiaB2.Carrier.Database.User;

//TODO: UserName can not be set. We use SteamId instead. But we need to call AddMissingData() to set the correct values. This is a hack.
public class PolydystopiaUserRepository : IPolydystopiaUserRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public PolydystopiaUserRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PolytopiaUserViewModel> GetBySteamIdAsync(SteamID steamId, string username)
    {
        var model = await _dbContext.Users.FirstOrDefaultAsync(u => u.SteamId == steamId.ToString());

        if (model == null)
        {
            model = await CreateAsync(steamId, username);
        }

        return model;
    }

    public async Task<PolytopiaUserViewModel?> GetByIdAsync(Guid polytopiaId)
    {
        var model = await _dbContext.Users.FindAsync(polytopiaId) ?? null;

        return model;
    }

    public async Task<PolytopiaUserViewModel> CreateAsync(SteamID steamId, string username)
    {
        var steamIdString = steamId.ToString();

        var user = new PolytopiaUserViewModel();

        user.PolytopiaId = Guid.NewGuid();
        user.UserName = username;
        user.Alias = username; //TODO: Find out if needed
        user.FriendCode = "12345678"; //TODO: Generate random friend code
        user.AllowsFriendRequests = true;
        user.SteamId = steamIdString;
        user.NumFriends = 0;
        user.Elo = 1000;
        user.Victories = new Dictionary<string, int>();
        user.Defeats = new Dictionary<string, int>();
        user.NumGames = 0;
        user.NumMultiplayergames = 0;
        user.MultiplayerRating = 1000;
        user.AvatarStateData =
            Convert.FromBase64String(
                "YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA"); //TODO: Reverse avatar state
        user.UserMigrated = true;
        user.GameVersions = new List<ClientGameVersionViewModel>();
        user.GameVersions = new List<ClientGameVersionViewModel> //TODO: Get real game versions
        {
            new()
            {
                Platform = Platform.Steam,
                DeviceId = "4c24759ff9d1d0c6e8bb28c7afc178b4752eca0d",
                GameVersion = 112
            }
        };
        user.LastLoginDate = DateTime.Now;
        user.UnlockedTribes = new List<int>();
        user.UnlockedSkins = new List<int>();
        user.CmUserData = new UserViewModel(); //TODO: Check if needed

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<List<PolytopiaUserViewModel>> GetAllByNameStartsWith(string name)
    {
        var foundUsers = await _dbContext.Users
            .WhereUserNameStartsWith(name)
            .ToListAsync();

        return foundUsers;
    }
}

public static class QueryExtensions
{
    public static IQueryable<PolytopiaUserViewModel> WhereUserNameStartsWith(
        this IQueryable<PolytopiaUserViewModel> query, string name)
    {
        return query.Where(user => EF.Property<string>(user, "UserName").StartsWith(name));
    }

    public static IQueryable<PolytopiaUserViewModel> WhereAliasStartsWith(
        this IQueryable<PolytopiaUserViewModel> query, string name)
    {
        return query.Where(user => EF.Property<string>(user, "Alias").StartsWith(name));
    }
}