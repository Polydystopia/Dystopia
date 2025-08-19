using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dystopia.Database.Friendship;
using Dystopia.Database.Game;
using Dystopia.Database.Highscore;
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.TribeRating;
using Dystopia.Database.WeeklyChallenge;
using Dystopia.Database.WeeklyChallenge.League;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Data;

namespace Dystopia.Database.User;

public class UserEntity
{
    [Key] public Guid Id { get; init; }

    public string SteamId { get; init; }

    [MaxLength(32)] public string UserName { get; init; }

    [MaxLength(4)] public string Discriminator { get; init; }

    [StringLength(36)] [NotMapped] public string Alias => UserName + "#" + Discriminator;

    public bool AllowsFriendRequests { get; set; }

    public virtual ICollection<LobbyParticipatorUserEntity> LobbyParticipations { get; set; }
        = new List<LobbyParticipatorUserEntity>();

    public virtual ICollection<GameParticipatorUserUser> GameParticipations { get; set; }
        = new List<GameParticipatorUserUser>();

    public virtual ICollection<GameEntity> FavoriteGames { get; set; } = new List<GameEntity>();

    [NotMapped]
    public virtual ICollection<GameEntity> ActualFavoriteGames
    {
        get
        {
            if (GameCache.Cache == null) return FavoriteGames;

            var result = new List<GameEntity>();
            foreach (var game in FavoriteGames)
            {
                if (GameCache.Cache.TryGet(game.Id, out var cachedGame) && cachedGame != null)
                {
                    result.Add(cachedGame);
                }
                else
                {
                    result.Add(game);
                }
            }

            return result;
        }
    }

    public int Elo { get; set; }

    public byte[] AvatarStateData { get; set; }

    public List<ClientGameVersionViewModel> GameVersions { get; set; }

    public DateTime LastLoginDate { get; init; }

    public virtual ICollection<FriendshipEntity> Friends { get; set; } = new List<FriendshipEntity>();

    public virtual ICollection<HighscoreEntity> Highscores { get; set; } = new List<HighscoreEntity>();
    public virtual ICollection<TribeRatingEntity> TribeRatings { get; set; } = new List<TribeRatingEntity>();

    public virtual ICollection<WeeklyChallengeEntryEntity> WeeklyChallengeEntries { get; set; } = new List<WeeklyChallengeEntryEntity>();
    
    public int? CurrentLeagueId { get; set; }
    public virtual LeagueEntity? CurrentLeague { get; set; }
}

public static class LobbyGameMappingExtensions
{
    public static PolytopiaUserViewModel ToViewModel(this UserEntity e)
    {
        return new PolytopiaUserViewModel
        {
            PolytopiaId = e.Id,
            UserName = e.UserName,
            Alias = e.Alias,
            FriendCode = "", // Not supported
            AllowsFriendRequests = e.AllowsFriendRequests,
            SteamId = e.SteamId,
            NumFriends = e.Friends.Count,
            Elo = e.Elo,
            Victories = new Dictionary<string, int>(), //TODO
            Defeats = new Dictionary<string, int>(), //TODO
            NumGames = e.GameParticipations.Count,
            NumMultiplayergames = e.GameParticipations.Count,
            MultiplayerRating = e.Elo,
            AvatarStateData = e.AvatarStateData,
            UserMigrated = true,
            GameVersions = e.GameVersions,
            LastLoginDate = DateTime.MinValue,
            UnlockedTribes = new List<int>(),
            UnlockedSkins = new List<int>(),
            CmUserData = new UserViewModel(), //TODO check used
        };
    }
}

public static class UserCollectionMappingExtensions
{
    public static List<PolytopiaUserViewModel> ToViewModels(this IEnumerable<UserEntity>? source) =>
        source?.Select(e => e.ToViewModel()).ToList() ?? new List<PolytopiaUserViewModel>();
}