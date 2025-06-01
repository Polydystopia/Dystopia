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

    /// <summary>
    /// Retrieves a user by Steam ID asynchronously. If the user does not exist, a new user is created.
    /// </summary>
    /// <param name="steamId">The SteamID of the user to retrieve or create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user information.</returns>
    public async Task<PolytopiaUserViewModel> GetBySteamIdAsync(SteamID steamId)
    {
        var model = await _dbContext.Users.FirstOrDefaultAsync(u => u.SteamId == steamId.ToString());

        if (model == null)
        {
            model = await CreateAsync(steamId);
        }

        return AddMissingData(model);
    }

    public async Task<PolytopiaUserViewModel?> GetByIdAsync(Guid polytopiaId)
    {
        var model = await _dbContext.Users.FindAsync(polytopiaId) ?? null;

        return AddMissingData(model);
    }

    /// <summary>
    /// Creates a new user record asynchronously based on the provided Steam ID.
    /// </summary>
    /// <param name="steamId">The SteamID used to create the new user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created user information.</returns>
    public async Task<PolytopiaUserViewModel> CreateAsync(SteamID steamId)
    {
        var steamIdString = steamId.ToString();

        var user = new PolytopiaUserViewModel();

        user.PolytopiaId = Guid.NewGuid();
        user.UserName = steamIdString; //TODO: Get real name looks like not saved to db
        user.Alias = steamIdString; //TODO: Get real name looks like not saved to db
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
        var foundUsers = await _dbContext.Users.Where(user => user.SteamId.StartsWith(name)).ToListAsync();

        return AddMissingData(foundUsers);
    }

    public static PolytopiaUserViewModel AddMissingData(PolytopiaUserViewModel? user)
    {
        if (user == null) return null;

        user.UserName = user.SteamId;
        user.Alias = user.SteamId;

        return user;
    }

    public static  List<PolytopiaUserViewModel> AddMissingData(List<PolytopiaUserViewModel> users)
    {
        foreach (var user in users)
        {
            AddMissingData(user);
        }

        return users;
    }
}