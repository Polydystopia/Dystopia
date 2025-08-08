using System.ComponentModel.DataAnnotations.Schema;
using Dystopia.Database.Game;
using Dystopia.Database.Lobby;
using Dystopia.Services.Cache;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.User;

public abstract class ParticipatorUserEntity
{
    public Guid UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;

    public PlayerInvitationState InvitationState { get; set; }

    public int SelectedTribe { get; set; }
    public int SelectedTribeSkin { get; set; }
}

public class LobbyParticipatorUserEntity : ParticipatorUserEntity
{
    public Guid LobbyId { get; set; }
    public virtual LobbyEntity Lobby { get; set; } = null!;
}

public class GameParticipatorUserEntity : ParticipatorUserEntity
{
    private static ICacheService<GameEntity>? _gameCache;

    public static void InitializeCache(ICacheService<GameEntity> cache)
    {
        _gameCache = cache;
    }

    public Guid GameId { get; set; }

    [Obsolete("Use ActualGame property instead - this may contain stale data", error: false)]
    protected virtual GameEntity Game { get; set; } = null!;

    public DateTime? DateLastCommand { get; set; }
    public DateTime? DateLastStartTurn { get; set; }
    public DateTime? DateLastEndTurn { get; set; }
    public DateTime? DateCurrentTurnDeadline { get; set; }
    public TimeSpan? TimeBank { get; set; }
    public TimeSpan? LastConsumedTimeBank { get; set; }

    [NotMapped]
    public GameEntity ActualGame
    {
        get
        {
            if (_gameCache != null && _gameCache.TryGet(GameId, out var cachedGame) && cachedGame != null)
            {
                return cachedGame;
            }

            return Game;
        }
    }
}

public static class ParticipatorUserEntityExtensions
{
    public static ParticipatorViewModel ToViewModel(this LobbyParticipatorUserEntity e)
    {
        return new ParticipatorViewModel
        {
            UserId = e.UserId,
            Name = e.User.Alias,
            NumberOfFriends = e.User.Friends.Count,
            NumberOfMultiplayerGames = e.User.GameParticipations.Count,
            GameVersion = e.User.GameVersions,
            MultiplayerRating = e.User.Elo,
            InvitationState = e.InvitationState,
            SelectedTribe = e.SelectedTribe,
            SelectedTribeSkin = e.SelectedTribeSkin,
            AvatarStateData = e.User.AvatarStateData,
        };
    }

    public static ParticipatorViewModel ToViewModel(this GameParticipatorUserEntity e)
    {
        return new ParticipatorViewModel
        {
            UserId = e.UserId,
            Name = e.User.Alias,
            NumberOfFriends = e.User.Friends.Count,
            NumberOfMultiplayerGames = e.User.GameParticipations.Count,
            GameVersion = e.User.GameVersions,
            MultiplayerRating = e.User.Elo,
            DateLastCommand = e.DateLastCommand,
            DateLastStartTurn = e.DateLastStartTurn,
            DateLastEndTurn = e.DateLastEndTurn,
            DateCurrentTurnDeadline = e.DateCurrentTurnDeadline,
            TimeBank = e.TimeBank,
            LastConsumedTimeBank = e.LastConsumedTimeBank,
            InvitationState = e.InvitationState,
            SelectedTribe = e.SelectedTribe,
            SelectedTribeSkin = e.SelectedTribeSkin,
            AvatarStateData = e.User.AvatarStateData,
        };
    }
}

public static class ParticipatorUserCollectionMappingExtensions
{
    public static List<ParticipatorViewModel> ToViewModels(this IEnumerable<LobbyParticipatorUserEntity>? source) =>
        source?.Select(e => e.ToViewModel()).ToList() ?? new List<ParticipatorViewModel>();


    public static List<ParticipatorViewModel> ToViewModels(this IEnumerable<GameParticipatorUserEntity>? source) =>
        source?.Select(e => e.ToViewModel()).ToList() ?? new List<ParticipatorViewModel>();
}

public static class CollectionExtensions
{
    public static void RemoveAll<T>(this ICollection<T> coll, Func<T,bool> predicate)
    {
        foreach (var item in coll.Where(predicate).ToList())
            coll.Remove(item);
    }
}