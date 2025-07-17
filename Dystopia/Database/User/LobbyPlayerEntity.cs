using System.ComponentModel.DataAnnotations;
using Dystopia.Database.Lobby;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.User;

public class LobbyPlayerEntity
{
    // ParticipatorViewModel ripoff but it doesn't duplicate user fields
    [Key]
    public required Guid UserId { get; init; }
    public UserEntity? User { get; init; } = null!;
    public required Guid LobbyId { get; init; }
    public LobbyEntity? Lobby { get; init; } = null!;
    public required DateTime? DateLastCommand { get; init; }
    public required DateTime? DateLastStartTurn { get; init; }

    public required DateTime? DateLastEndTurn { get; init; }

    public required DateTime? DateCurrentTurnDeadline { get; init; }

    public required TimeSpan? TimeBank { get; init; }

    public required TimeSpan? LastConsumedTimeBank { get; init; }
    public required PlayerInvitationState InvitationState { get; set; }

    public required int SelectedTribe { get; set; }

    public required int SelectedTribeSkin { get; set; }
    public required int AutoSkipStrikeCount { get; init; }

    public static explicit operator ParticipatorViewModel(LobbyPlayerEntity v)
    {
        return new ParticipatorViewModel
        {
            UserId = v.UserId,
            Name = v.User.Alias,
            NumberOfFriends = v.User.NumFriends,
            NumberOfMultiplayerGames = v.User.NumGames,
            GameVersion = new List<ClientGameVersionViewModel>(),
            MultiplayerRating = v.User.Elo,
            DateLastCommand = v.DateLastCommand,
            DateLastStartTurn = v.DateLastStartTurn,
            DateLastEndTurn = v.DateLastEndTurn,
            DateCurrentTurnDeadline = v.DateCurrentTurnDeadline,
            TimeBank = v.TimeBank,
            LastConsumedTimeBank = v.LastConsumedTimeBank,
            InvitationState = v.InvitationState,
            SelectedTribe = v.SelectedTribe,
            SelectedTribeSkin = v.SelectedTribeSkin,
            HasFailedParse = false,
            AvatarStateData = v.User.AvatarStateData,
            AutoSkipStrikeCount = v.AutoSkipStrikeCount
        };
    }
    public static explicit operator LobbyPlayerEntity(ParticipatorViewModel v)
    {
        return new LobbyPlayerEntity
        {
            UserId = v.UserId,
            DateLastCommand = v.DateLastCommand,
            DateLastStartTurn = v.DateLastStartTurn,
            DateLastEndTurn = v.DateLastEndTurn,
            DateCurrentTurnDeadline = v.DateCurrentTurnDeadline,
            TimeBank = v.TimeBank,
            LastConsumedTimeBank = v.LastConsumedTimeBank,
            InvitationState = v.InvitationState,
            SelectedTribe = v.SelectedTribe,
            SelectedTribeSkin = v.SelectedTribeSkin,
            AutoSkipStrikeCount = v.AutoSkipStrikeCount
        };
    }

}