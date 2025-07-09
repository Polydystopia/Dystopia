using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dystopia.Database.Friendship;
using Dystopia.Database.Game;
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using PolytopiaBackendBase.Auth;
using SteamKit2;

namespace Dystopia.Database.User;
public class UserEntity
{
    [Key]
    public Guid PolytopiaId { get; init; }
    [MaxLength(32)] // 32 for the username, 4 for the discriminator
    public required string Alias { get; init; }

    [StringLength(4)]
    public required string Discriminator { get; init; }

    [StringLength(36)]
    [NotMapped]
    public string UserName => Alias + Discriminator;

    public required SteamID SteamId { get; init; } // TODO owns-a
    // idk why you would use friendCode
    public required bool AllowsFriendRequests { get; init; }
    public required int Elo { get; init; }
    public required List<Guid> GameIds { get; init; } = new List<Guid>();
    public ICollection<LobbyPlayerEntity> Games { get; init; } = null!;
    public ICollection<MatchmakingEntity> MatchmakingEntities { get; init; } = null!;
    [NotMapped]
    public int NumGames => Games.Count;
    public required byte[] AvatarStateData { get; set; }
    public required DateTime LastLoginDate { get; init; } // we dont transmit it but we do store it
    // public ICollection<FriendshipEntity> Friends { get; set; } = null!; // we cant have this because a friend has user1 and 2
    public ICollection<FriendshipEntity> Friends1 { get; init; } = null!;
    public ICollection<FriendshipEntity> Friends2 { get; init; } = null!;
    [NotMapped]
    public int NumFriends => Friends1.Count + Friends2.Count;
    
    public static explicit operator PolytopiaUserViewModel(UserEntity v)
    {
        return new PolytopiaUserViewModel
        {
            PolytopiaId = v.PolytopiaId,
            UserName = v.UserName,
            Alias = v.Alias,
            FriendCode = String.Empty,
            AllowsFriendRequests = v.AllowsFriendRequests,
            SteamId = v.Alias,
            NumFriends = v.NumFriends,
            Elo = v.Elo,
            Victories = new(), // TODO,
            Defeats = new() , // TODO
            NumGames = v.NumGames,
            NumMultiplayergames = v.NumGames,
            MultiplayerRating = v.Elo,
            AvatarStateData = v.AvatarStateData,
            UserMigrated = true,
            GameVersions = new List<ClientGameVersionViewModel>(),
            UnlockedTribes = new(),
            UnlockedSkins = new(),
            CmUserData = null,
        };
    }
    // no reverse as thats not possible
}