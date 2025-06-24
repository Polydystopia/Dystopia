using DystopiaShared;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;

public class LobbyMapping
{
    public static SharedLobbyGameViewModel Map(LobbyGameViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new SharedLobbyGameViewModel
        {
            Id = src.Id,
            UpdatedReason = (SharedLobbyUpdatedReason)src.UpdatedReason,
            DateCreated = src.DateCreated,
            DateModified = src.DateModified,
            Name = src.Name,
            MapPreset = (SharedMapPreset)src.MapPreset,
            MapSize = src.MapSize,
            OpponentCount = src.OpponentCount,
            GameMode = (SharedGameMode)src.GameMode,
            OwnerId = src.OwnerId,
            DisabledTribes = new List<int>(src.DisabledTribes),
            StartedGameId = src.StartedGameId,
            IsPersistent = src.IsPersistent,
            IsSharable = src.IsSharable,
            TimeLimit = src.TimeLimit,
            ScoreLimit = src.ScoreLimit,
            InviteLink = src.InviteLink,
            MatchmakingGameId = src.MatchmakingGameId,
            ChallengermodeGameId = src.ChallengermodeGameId,
            StartTime = src.StartTime,
            GameContext = Map(src.GameContext),
            Participators = src.Participators
                .Select(p => Map(p))
                .ToList(),
            Bots = new List<int>(src.Bots)
        };
    }

    public static SharedGameContext Map(GameContext? src)
    {
        if (src is null) return null!;
        return new SharedGameContext
        {
            ExternalTournamentId = src.ExternalTournamentId,
            ExternalMatchId = src.ExternalMatchId
        };
    }

    public static SharedParticipatorViewModel Map(ParticipatorViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new SharedParticipatorViewModel
        {
            UserId = src.UserId,
            Name = src.GetNameInternal(),
            NumberOfFriends = src.NumberOfFriends,
            NumberOfMultiplayerGames = src.NumberOfMultiplayerGames, GameVersion = src.GameVersion
                .Select(v => Map(v))
                .ToList(),
            MultiplayerRating = src.MultiplayerRating,
            DateLastCommand = src.DateLastCommand,
            DateLastStartTurn = src.DateLastStartTurn,
            DateLastEndTurn = src.DateLastEndTurn,
            DateCurrentTurnDeadline = src.DateCurrentTurnDeadline,
            TimeBank = src.TimeBank,
            LastConsumedTimeBank = src.LastConsumedTimeBank,
            InvitationState = (SharedPlayerInvitationState)src.InvitationState,
            SelectedTribe = src.SelectedTribe,
            SelectedTribeSkin = src.SelectedTribeSkin,
            HasFailedParse = src.HasFailedParse,
            AvatarStateData = src.AvatarStateData,
            AutoSkipStrikeCount = src.AutoSkipStrikeCount
        };
    }

    public static SharedClientGameVersionViewModel Map(ClientGameVersionViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new SharedClientGameVersionViewModel
        {
            Platform = (SharedPlatform)src.Platform,
            DeviceId = src.DeviceId,
            GameVersion = src.GameVersion
        };
    }
}